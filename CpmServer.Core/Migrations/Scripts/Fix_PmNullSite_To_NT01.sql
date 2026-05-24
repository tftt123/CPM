-- Fix PM module records with NULL Site to 'NT01'
-- Run this in SSMS or Azure Data Studio against the CPM database

BEGIN TRANSACTION;

UPDATE PmProjectTrace
SET Site = 'NT01'
WHERE Site IS NULL;
PRINT 'Updated PmProjectTrace: ' + CAST(@@ROWCOUNT AS VARCHAR);

UPDATE PmProjectTraceStep
SET Site = 'NT01'
WHERE Site IS NULL;
PRINT 'Updated PmProjectTraceStep: ' + CAST(@@ROWCOUNT AS VARCHAR);

UPDATE PmProjectTraceStepActualCycleTime
SET Site = 'NT01'
WHERE Site IS NULL;
PRINT 'Updated PmProjectTraceStepActualCycleTime: ' + CAST(@@ROWCOUNT AS VARCHAR);

UPDATE PmStepCycleTimeChangeRequest
SET Site = 'NT01'
WHERE Site IS NULL;
PRINT 'Updated PmStepCycleTimeChangeRequest: ' + CAST(@@ROWCOUNT AS VARCHAR);

UPDATE PmStepCycleTimeChangeDetail
SET Site = 'NT01'
WHERE Site IS NULL;
PRINT 'Updated PmStepCycleTimeChangeDetail: ' + CAST(@@ROWCOUNT AS VARCHAR);

COMMIT;
PRINT 'Done.';
