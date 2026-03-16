# Query Types Quick Reference

## CSV Header
```csv
sequence,entityname,start date,end date,entityidcolumn,querytype,additionalquery
```

## Query Type Examples

### 1. Standard Query (Default)
```csv
1,contact,01-01-2024,31-12-2024,contactid,Standard,
```
OR with filter:
```csv
2,contact,01-01-2024,31-12-2024,contactid,Standard,statecode = 1
```

### 2. SQL Query
```csv
3,indskr_customercallplan,01-01-2024,31-12-2024,indskr_customercallplanid,SQL,"SELECT indskr_customercallplanid FROM indskr_customercallplan icp INNER JOIN contact c ON c.contactid = icp.indskr_customerid WHERE c.indskr_externalid NOT LIKE 'MDM%'"
```

### 3. FetchXML Query
```csv
4,appointment,01-01-2024,31-12-2024,appointmentid,FetchXML,"<fetch><entity name='appointment'><attribute name='appointmentid'/><filter type='and'><condition attribute='subject' operator='like' value='%Test%'/></filter></entity></fetch>"
```

## When to Use Each Type

| Query Type | Use When | Pros | Cons |
|------------|----------|------|------|
| **Standard** | Simple date-based deletion | Fast, easy | Limited filtering |
| **SQL** | Complex JOINs, performance critical | Fastest, flexible | Security risk, needs SQL access |
| **FetchXML** | Dynamics-native queries | Safe, native | Verbose XML syntax |

## Sample Files
- `sample_deletions.csv` - Standard queries
- `sample_with_joins.csv` - SQL query example  
- `sample_fetchxml.csv` - FetchXML examples
- `sample_custom_queries.csv` - Mixed query types

## Key Points
✅ First column must return entity ID (GUID)
✅ Entity name always required for deletion API
✅ Wrap query text in double quotes in CSV
⚠️ SQL queries have injection risk - use trusted sources only
⚠️ Test queries before running deletions
