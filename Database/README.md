# dbVANestle – database review and constraint scripts

Review date: 2026-10-06 · Database: `dbVANestle` on SQL Server 2022 Express (compatibility level 140), 81 tables, 253k rows, 136 MB.

## Findings

| # | Finding | Impact | Addressed by |
|---|---------|--------|--------------|
| 1 | **No foreign keys** in the whole database | Nothing stops rows pointing at enquiries, items, suppliers or users that do not exist. 9 supplier quotes already belong to deleted line items. | `04` (+ `06` A) |
| 2 | **35 tables have no primary key** (heaps), incl. look-ups the API joins on (`tblJobStatus`, `TBLMANAGMENTTYPE`, `tblAttachments`, `tblSurvey`) | Silent duplicates, full scans, cannot be referenced by a foreign key | `02` |
| 3 | **Almost no indexes**: `tblJobHistory` (57k rows, checked once per dashboard row) and `tblLineitemsHistory` (140k rows) are scanned on every request | Slow dashboard / project summary | `03` |
| 4 | **No CHECK / UNIQUE constraints**: status codes, step, job type and enquiry numbers are unchecked | Invalid codes and duplicate enquiry numbers are possible | `05` |
| 5 | **10 duplicated login names (30 accounts)**, case-insensitive (e.g. `New.Client` ×12, `Mitsuhiko.Toda` is both a client and an agency user) | Login signs in to an arbitrary one of the accounts | `06` B (decision) |
| 6 | **`0` used to mean "none"** in 10 key columns (e.g. `tblEnquiryMaster.ClientContactId` 119 rows) | A foreign key rejects 0; needs 0 → NULL together with an API/procedure change | `06` E (later) |
| 7 | `tblEnquiryDetails.Status` holds codes from **two lists** (`tblJobStatus` for Enquiry/Catalog, `EBtblJobStatus` for E-Bidding) with **different meanings for the same number** (0 = Draft vs Live, 1 = RFQ Sent vs Paused) | A foreign key is impossible; a range check is used instead | `05` |
| 8 | Duplicate item numbers inside an enquiry: `EN_DeleteItem` does not renumber and the API numbered new items `COUNT + 1` | Quotes and savings (matched by item number) attach to the wrong item | API fixed (`MAX + 1`); `06` C |
| 9 | Numbers and dates stored as text (`TaxPercentage`, `DefaultSalesPercentage`, `EuroRate`, `PreviousPrice`, `QuoteQtyOrSize`, `EstDate`, …) and country stored by **name** in many tables | Conversion errors, culture-dependent parsing, no referential integrity | Recommendation (data-type migration, separate project) |
| 10 | Passwords stored in **plain text** (`tblUser.Password`, `tblpwdhistory.password`), compared case-insensitively by the collation | Security | Recommendation (hashing; needs API + data migration) |
| 11 | `READ_COMMITTED_SNAPSHOT` **off** | Readers and writers block each other: the main source of "hanging" screens and reader/writer deadlocks | `07` (DBA) |
| 12 | Recovery model **SIMPLE** | No point-in-time restore | `07` note (DBA, with a log-backup job) |
| 13 | 10 backup/temp tables (`*_02042026`, `temp*`, `*old*`, …) not used by any code | Clutter, confusion | `06` G (archive, then drop) |
| 14 | Server is **SQL Server Express** (limited memory/CPU, no online index builds) | Performance ceiling under load | Infrastructure decision |

Found in the API while reviewing the data (not database issues, reported for decision):
- **Dashboard hides enquiries.** `ViewDashboard` joins the creator on `tblAgencyUsers.Userid`, but `tblEnquiryMaster.CreatedBy` holds `tblAgencyUsers.Id` (verified: 0 mismatches on `Id`, 1,143 on `Userid`). **817 active enquiries never appear on the dashboard**, e.g. everything created by users whose `Userid` is empty. `GetDetails` and the PDF already use `Id`.
- For E-Bidding (`menuid = 2`) the dashboard takes status names from `tblJobStatus` instead of `EBtblJobStatus`, so E-Bidding statuses are labelled wrongly and items in status -1/-2 are dropped.

## Scripts (`Scripts/`)

| Script | Changes data? | What it does |
|--------|---------------|--------------|
| `01_preflight_checks.sql` | No (read-only) | Lists every row that blocks a constraint |
| `02_primary_keys.sql` | Schema only | 24 primary keys on heaps that already have a unique, non-null key column |
| `03_indexes.sql` | Schema only | 20 indexes: foreign-key columns and the API's filter columns |
| `04_foreign_keys.sql` | Schema only | 22 foreign keys (21 immediately; quotes → items after `06` A) |
| `05_check_and_unique_constraints.sql` | Schema only | 5 CHECK constraints, unique enquiry number (`EnqUId`) and country name |
| `06_data_cleanup_REQUIRES_APPROVAL.sql` | **Yes** – dry run by default | A: removes the 9 orphan quotes (backed up first). B–G: reports needing a business decision |
| `07_database_settings.sql` | DB setting | Enables `READ_COMMITTED_SNAPSHOT` (needs a maintenance window) |
| `99_rollback.sql` | Schema only | Removes everything 02–05 created |
| `../Tests/behaviour_tests.sql` | No (all rolled back) | Proves the procedures still work and bad data is rejected |

Design rules used by every script:
- **Self-checking:** each constraint is created only if the existing data already satisfies it; otherwise it is skipped with the number of offending rows. Running a script never fails because of data.
- **Idempotent and transactional:** safe to re-run; a script either applies completely or not at all.
- **Delete behaviour matches the procedures:** enquiries are only soft-deleted (`Status = 0`) → `NO ACTION`; line items are hard-deleted by `EN_DeleteItem`/`EB_DeleteItem`, which delete the item *before* its specifications → the item's specifications, quotes, first-round quotes and savings use `ON DELETE CASCADE` (this also stops new orphan quotes). Audit/history tables reference only the enquiry, so history is kept.
- **Filtered unique index on `EnqUId`:** `EnqUId` is NULL for a moment while an enquiry is created. Filtered indexes require `QUOTED_IDENTIFIER`/`ANSI_NULLS ON` for writers: verified ON for all 345 procedures, triggers and functions.

## How to apply

1. Restore a copy of production to UAT and run everything there first.
2. `01` → review the output.
3. `02`, `03`, `04`, `05` (in this order) during a quiet period. Each prints `ADDED` / `OK` / `SKIP <reason>`.
4. `../Tests/behaviour_tests.sql` → every row must be `PASS`.
5. Business approves `06` section A → set `@Apply = 1`, run, then re-run `04` (adds the quotes → items key).
6. DBA: `07` in a maintenance window.
7. Rollback at any time: `99_rollback.sql` (and `SET READ_COMMITTED_SNAPSHOT OFF` for 07).

## Verification performed

The scripts were **not** run on the shared server. They were verified on a full private replica in SQL Server LocalDB (all 81 tables with keys, indexes and defaults, all 253,322 rows, all 345 procedures/functions/views/triggers):

- `02`–`05`: 24 PKs, 20 indexes, 21 FKs, 7 CHECK/UNIQUE created; quotes → items FK skipped with "9 rows reference a missing tblEnquiryDetails". Re-run: everything `OK`, nothing changed.
- `06` dry run: rolled back completely. Applied: 9 quotes backed up and removed; re-running `04` then added the remaining FK.
- `behaviour_tests.sql`: 9/9 PASS (`EN_DeleteItem` removed an item with 3 quotes, 5 specifications and 1 saving via cascade; `EB_DeleteItem`, `DeleteEnquiries`, `UpdateJobStatus` and the API create pattern succeed; orphan item, unknown status, duplicate `EnqUId` and unknown supplier are rejected).
- The API was run against the constrained replica: an end-to-end enquiry lifecycle through 27 endpoints/steps (create → project → 3 items → delete item → add item → suppliers → submit → quotes → savings → reasons → reference prices → fee → status → delivery address → brand → new client → duplicate item → PDF) passed 30/30 checks with 0 errors in the API log.
- `99_rollback.sql` restored the schema exactly (45 PKs, 11 indexes, 0 FKs, 0 checks, 45 defaults); re-applying afterwards created all 73 objects again.

## Decisions needed

1. Approve `06` A (remove 9 orphan quotes – they belong to deleted items and cannot be shown).
2. Resolve the 10 duplicated login names (`06` B), then make `UserName` unique.
3. Brands → Division delete rule: block deleting a division that has brands, or delete its brands too (`06` F).
4. Fix the dashboard creator join (API) – it changes which enquiries users see (817 more appear).
5. Plan the larger items: 0 → NULL for optional keys, text → proper numeric/date types, password hashing, RCSI, recovery model.
