# Custom SQL and FetchXML Queries Guide

## ✨ New Feature: Full SQL and FetchXML Query Support

The application now supports **three query types** for fetching records:
1. **Standard** - Date-based queries with optional filters
2. **SQL** - Custom SQL queries with JOINs, subqueries, etc.
3. **FetchXML** - Dynamics 365 native FetchXML queries

## 🎯 Query Type Selection

### CSV Format
```csv
sequence,entityname,start date,end date,entityidcolumn,querytype,additionalquery
```

### Query Types

#### 1. Standard Query (Default)
Uses date range with optional WHERE clause additions:
```csv
sequence,entityname,start date,end date,entityidcolumn,querytype,additionalquery
1,contact,01-01-2024,31-12-2024,contactid,Standard,
2,contact,01-01-2024,31-12-2024,contactid,Standard,statecode = 0
```

Generated SQL:
```sql
SELECT contactid 
FROM contact WITH (NOLOCK)
WHERE CreatedOn >= '2024-01-01' AND CreatedOn < '2024-12-31'
  AND statecode = 0
```

#### 2. SQL Query (Custom)
Full custom SQL with complete control:
```csv
sequence,entityname,start date,end date,entityidcolumn,querytype,additionalquery
3,indskr_customercallplan,01-01-2024,31-12-2024,indskr_customercallplanid,SQL,"SELECT indskr_customercallplanid FROM indskr_customercallplan icp INNER JOIN contact c ON c.contactid = icp.indskr_customerid WHERE c.indskr_externalid NOT LIKE 'MDM%'"
```

#### 3. FetchXML Query (Dynamics Native)
Use Dynamics 365 FetchXML syntax:
```csv
sequence,entityname,start date,end date,entityidcolumn,querytype,additionalquery
4,appointment,01-01-2024,31-12-2024,appointmentid,FetchXML,"<fetch><entity name='appointment'><attribute name='appointmentid'/><filter type='and'><condition attribute='subject' operator='like' value='%Test%'/></filter></entity></fetch>"
```

## 📋 Use Cases

### SQL Queries

#### 1. JOIN with Filter
Delete call plans where contact external ID doesn't start with 'MDM':
```csv
sequence,entityname,start date,end date,entityidcolumn,querytype,additionalquery
1,indskr_customercallplan,01-01-2024,31-12-2024,indskr_customercallplanid,SQL,"SELECT indskr_customercallplanid FROM indskr_customercallplan icp INNER JOIN contact c ON c.contactid = icp.indskr_customerid WHERE c.indskr_externalid NOT LIKE 'MDM%'"
```

#### 2. Multiple JOINs
Delete records based on related entity data:
```csv
sequence,entityname,start date,end date,entityidcolumn,querytype,additionalquery
2,appointment,01-01-2024,31-12-2024,appointmentid,SQL,"SELECT appointmentid FROM appointment a INNER JOIN contact c ON a.regardingobjectid = c.contactid INNER JOIN account acc ON c.parentcustomerid = acc.accountid WHERE acc.accountcategorycode = 2"
```

#### 3. Subquery
Delete records matching a subquery condition:
```csv
sequence,entityname,start date,end date,entityidcolumn,querytype,additionalquery
3,phonecall,01-01-2024,31-12-2024,phonecallid,SQL,"SELECT phonecallid FROM phonecall WHERE regardingobjectid IN (SELECT contactid FROM contact WHERE address1_country = 'USA')"
```

### FetchXML Queries

#### 1. Simple Filter
Delete appointments with specific subject:
```csv
sequence,entityname,start date,end date,entityidcolumn,querytype,additionalquery
4,appointment,01-01-2024,31-12-2024,appointmentid,FetchXML,"<fetch><entity name='appointment'><attribute name='appointmentid'/><filter type='and'><condition attribute='subject' operator='like' value='%Test%'/></filter></entity></fetch>"
```

#### 2. Link Entity (JOIN)
Delete call plans where contact has specific attribute:
```csv
sequence,entityname,start date,end date,entityidcolumn,querytype,additionalquery
5,indskr_customercallplan,01-01-2024,31-12-2024,indskr_customercallplanid,FetchXML,"<fetch><entity name='indskr_customercallplan'><attribute name='indskr_customercallplanid'/><link-entity name='contact' from='contactid' to='indskr_customerid'><filter><condition attribute='indskr_externalid' operator='not-like' value='MDM%'/></filter></link-entity></entity></fetch>"
```

#### 3. Multiple Conditions
Delete records with complex criteria:
```csv
sequence,entityname,start date,end date,entityidcolumn,querytype,additionalquery
6,contact,01-01-2024,31-12-2024,contactid,FetchXML,"<fetch><entity name='contact'><attribute name='contactid'/><filter type='and'><condition attribute='statecode' operator='eq' value='0'/><condition attribute='createdon' operator='last-x-months' value='6'/><condition attribute='emailaddress1' operator='null'/></filter></entity></fetch>"
```

## ⚙️ CSV Format Details

### Column Layout
```csv
sequence,entityname,start date,end date,entityidcolumn,querytype,additionalquery
```

### Columns Explained

| Column | Required | Description | Example |
|--------|----------|-------------|---------|
| **sequence** | Yes | Execution order | 1, 2, 3... |
| **entityname** | Yes | Entity logical name (for deletion) | contact, appointment |
| **start date** | Yes* | Start date (for Standard queries) | 01-01-2024 |
| **end date** | Yes* | End date (for Standard queries) | 31-12-2024 |
| **entityidcolumn** | Yes | ID column name | contactid |
| **querytype** | No | Query type: Standard, SQL, or FetchXML | SQL |
| **additionalquery** | No | Query text or filter | SELECT... or \<fetch\>... |

\* Dates are required in CSV but ignored for SQL and FetchXML query types

### Query Type Values

- `Standard` or empty - Standard date-based query
- `SQL` - Custom SQL query
- `FetchXML` - Dynamics FetchXML query

## 🔍 Query Type Comparison

| Feature | Standard | SQL | FetchXML |
|---------|----------|-----|----------|
| **Speed** | Fast (SQL backend) | Fastest | Medium (API) |
| **Complexity** | Simple filters | JOINs, subqueries | Link entities |
| **Security** | ⚠️ Low risk | ⚠️⚠️ SQL injection risk | ✅ Safe |
| **Permissions** | API access | SQL DB access | API access |
| **Date Range** | Built-in | Manual | Manual |
| **Best For** | Simple deletes | Complex SQL queries | Dynamics-native queries |

## 📝 Complete Examples

### Example CSV File
```csv
sequence,entityname,start date,end date,entityidcolumn,querytype,additionalquery
1,contact,01-01-2024,31-12-2024,contactid,Standard,
2,contact,01-01-2024,31-12-2024,contactid,Standard,statecode = 1
3,indskr_customercallplan,01-01-2024,31-12-2024,indskr_customercallplanid,SQL,"SELECT indskr_customercallplanid FROM indskr_customercallplan icp INNER JOIN contact c ON c.contactid = icp.indskr_customerid WHERE c.indskr_externalid NOT LIKE 'MDM%'"
4,appointment,01-01-2024,31-12-2024,appointmentid,FetchXML,"<fetch><entity name='appointment'><attribute name='appointmentid'/><filter type='and'><condition attribute='subject' operator='like' value='%Test%'/></filter></entity></fetch>"
```

See [sample_custom_queries.csv](sample_custom_queries.csv) for more examples.

## ⚠️ Important Notes

### For All Query Types
1. **First Column**: Must return the entity ID (GUID) to delete
2. **Entity Name**: Always required - used for deletion API calls
3. **Quotes**: Wrap query text in double quotes in CSV files
4. **Testing**: Always test queries before running deletions

### SQL Queries
- ⚠️ **Security Risk**: SQL injection possible - only use trusted queries
- ⚠️ **Permissions**: Requires direct SQL database access
- ✅ **Performance**: Fastest for large datasets with JOINs
- Use `WITH (NOLOCK)` for better read performance

### FetchXML Queries
- ✅ **Security**: Safe from injection attacks
- ✅ **Native**: Uses Dynamics 365 query language
- ⚠️ **Complexity**: XML syntax can be verbose
- ✅ **Paging**: Automatically handled by the application

### Standard Queries
- ✅ **Simple**: Easy to use for basic date ranges
- ✅ **Fast**: Uses SQL backend when configured
- ⚠️ **Limited**: Only supports simple WHERE clause additions

## 🚀 Best Practices

### Choosing Query Type

**Use Standard when:**
- Simple date-based deletions
- No complex filtering needed
- Quick setup required

**Use SQL when:**
- Complex JOINs between multiple tables
- Subqueries required
- Performance is critical
- You have SQL expertise

**Use FetchXML when:**
- Link entities (JOINs) with Dynamics relationships
- Need Dynamics-native operators
- Security is a concern
- Working within Dynamics 365 paradigm

### General Tips
1. **Test First**: Run your query manually to verify results
2. **Start Small**: Test with limited datasets
3. **Log Review**: Check logs for query execution details
4. **Backup**: Always backup data before mass deletions
5. **Escape Properly**: Use double quotes for CSV, escape internal quotes

## 🔧 FetchXML Tips

### Basic Structure
```xml
<fetch>
  <entity name="entityname">
    <attribute name="entityid" />
    <filter type="and">
      <condition attribute="fieldname" operator="eq" value="value" />
    </filter>
  </entity>
</fetch>
```

### Common Operators
- `eq` - equals
- `ne` - not equals
- `like` - contains (use % wildcards)
- `not-like` - doesn't contain
- `gt` / `lt` - greater than / less than
- `null` / `not-null` - is null / is not null
- `in` - in list of values
- `on` / `on-or-after` - date comparisons

### Link Entity (JOIN)
```xml
<fetch>
  <entity name="indskr_customercallplan">
    <attribute name="indskr_customercallplanid" />
    <link-entity name="contact" from="contactid" to="indskr_customerid">
      <filter>
        <condition attribute="indskr_externalid" operator="not-like" value="MDM%" />
      </filter>
    </link-entity>
  </entity>
</fetch>
```

### Generate FetchXML
You can generate FetchXML using:
- **Advanced Find** in Dynamics 365 (Download FetchXML)
- **FetchXML Builder** (XrmToolBox)
- **Power Apps** (View filters → Advanced Find)

## 🛠️ Troubleshooting

### SQL Queries

**Query Not Executing**
- Verify SQL connection is configured
- Check query syntax in SQL Management Studio
- Ensure column names are correct

**Wrong Records Returned**
- First column must return entity ID
- Check JOIN conditions
- Verify table names (case-sensitive in some DBs)

**Performance Issues**
- Add indexes on JOIN columns
- Use `WITH (NOLOCK)` for reads
- Limit result set for testing

### FetchXML Queries

**Invalid FetchXML Error**
- Validate XML syntax
- Ensure entity/attribute names are correct
- Check operator values are valid

**No Records Returned**
- Test FetchXML in Advanced Find first
- Verify filter conditions
- Check link entity relationships

**Paging Issues**
- Application handles paging automatically
- Don't add page/count attributes manually
- Check logs for paging details

---

## 📊 Sample Files

- [sample_with_joins.csv](sample_with_joins.csv) - SQL query example
- [sample_custom_queries.csv](sample_custom_queries.csv) - Multiple query types

## 🔗 Related Documentation

- [SQL_FETCH_FEATURE.md](SQL_FETCH_FEATURE.md) - SQL connection setup
- [README.md](README.md) - General application usage
- [UI_GUIDE.md](UI_GUIDE.md) - User interface guide

---

**Note**: The `start date` and `end date` columns are still required in the CSV format but are ignored for SQL and FetchXML query types. They are only used for Standard queries.
