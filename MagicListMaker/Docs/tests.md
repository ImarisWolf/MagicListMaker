# Testing the program
Throughout the whole test the error messages must be tracked. They must be SHORT, CORRECT, COMPLETE and CLEAR for understanding. If not - this should be fixed.

## I. Synthax testings
### Database declaration testings
**Check:**
+ works correctly if declaration is missing, but gives an error when try to make a list without a declaration;
+ an error if declaration close tag is missing;
+ an error if declaration is empty;
+ works correctly if declaration has one database;
+ works correctly if declaration has several databases;
+ the path values are valid only when they are in quotes;

### List declaration testings
**Check:**
+ works correctly if declaration is missing;
+ an error if declaration close tag is missing;
+ an error if databases are missing;
+ works correctly without options;
+ works correctly with one option;
+ works correctly with several options;
+ an error if one option is declared several times;
+ works correctly if declaration has one database;
+ works correctly if declaration has several databases;
+ list base synthax works correctly, and the program responds an error if there is a base synthax error;

### Base value testings
**Check:**
+ for databases:
  - works correctly if only one database is used;
  - works correctly if several databases are used;
  - an error if empty string is used in databases;
  - an error if a non-declared database is used;
+ for options:
  - all bool values are valid only when `true`, `false`, `0`, or `1` is set as a value, quotes are not applied;
  - all number values are valid only when an integer or a number is set as a value, quotes are not applied;
  - all string values are valid only when the value is in single quotes;
+ for filter:
  - the value is valid only in single quotes;
  - an error if the value is not a bool;
+ for grouping:
  - the value is valid only in quotes;
  - an error if sorting is missing;
  - an error if the value is not a set of field names enumeration, separeted by `,`;
+ for sorting:
  - the value is valid only in quotes;
  - an error if the value is not a field names enumeration;
+ for format:
  - the value is valid only in quotes;

## II. Functioning testings

### Database parsing testings
**Check:**
+ an error if the file doesn't exist;
+ an error if the path doesn't exist;
+ if the file and path exist, but the file path is unusual:
  - works correctly with both `/` and `\` symbols;
  - works correctly with double `/` or `\` symbol after drive letter;
  - works correctly with multi-letter drives;
  - works correctly if the file is on server (`\\Server\...` or `\\192.168...`);
+ if the file exists, but the content is incorrect:
  - an error if the file is empty;
  - an error if it has an incorrect first string (column string);
  - an error if it has an unregistered column;
  - an error if it has correct first line, but it's length doesn't match the length of other lines;
  - an error if it has correct first line, but incorrect other document in any other way;
+ if the file exists and it is correct:
  - works correctly if only one column is in input (use several different columns for that, including something very unusual);
  - works correctly if every column is in input;
  - works correctly with several columns as input;
  - works correctly with missing grades;
  - grades replacement works correctly;
  - works correctly even if the whole MA Database is loaded;
+ merge works correctly;

### Note parsing testings
**Check:**
+ works according to default if the parseComments option isn't set;
+ for `parseComments = false`, `parseComments = true` and a note was parsed, and `parseComments = true` and no note was parsed, check the following:
  - for every-field-export and for one-field-export:
    * every Magic Album field is written in appropriate card field;
    * `comments` field contains `note` field info without changes;
    * no field contains a null value;
+ for `parseComments = false`:
  - it separates foil and non-foil as usual;
+ for `parseComments = true`:
  - parse works with `,` and `;`;
  - parse works with `.` in the end;
  - works correctly when a parameter has a string like "NM/M with a stamp";
  - comments works correctly;
  - for discount:
    * `d10`, `d200` works correctly;
    * `10%`, `200%` works correctly;
    * `d-10`, `d-200` works correctly;
    * `-10%`, `-200%` works correctly;
  - for dollar rate:
    * `r50` works correctly;
    * `r0.30` works correctly;
    * an error for `r-50` or `r-0.30`;
  - any-field parameter works correctly;
  - several any-field parameters work correctly;
  - language parameter works correctly;
  - positive price works correctly;
  - an error when trying to set a negative price;
  - a parameter with all the options in any order works correctly;
  - an error for unexpected parameters;
  - it works correctly when parsing different types of parameters declared in any order;
  - an error if sum of qtys in parameters is bigger than qty of cards;
  - works correctly if sum of qtys in parameters is equal to qty of cards;

### Price and round testings
**Check:**
+ defaultDollarRate works correctly;
+ dollar rate in card field overrides defaultDollarRate;
+ discount in card field overrides defaultDiscount and all default discounts for grades;
+ in other cases, for graded cards, if default discount for these cards is not 0, default discounts for grades are used;
+ in any other cases, default discount is used;
+ smart round works correctly regardless of if round option is set or not;
+ if smartRound is off and round is different from 0, round works and does it correctly (with fractional numbers and integers as a parameter);
+ price that is less than 1 is rounded to 1;
+ all default options work according to default if the appropriate options aren't set;

### Other options testings
**Check:**
+ handleMultiNames works according to default if the handleMultiNames option isn't set;
+ works correctly when handleMultiNames is on;
+ works correctly when handleMultiNames is off;

### Sorting and grouping testings
**Check:**
+ works correctly with one sort field;
+ works correctly with one several sort fields;
+ reverse sorting works correctly;
+ works correctly with one group with one field;
+ works correctly with one group with several field;
+ works correctly with several groups;

### Filter and formatting testings
**Check:**
+ `true` and `false` values work correctly;
+ negative for `true` and `false` values (`!`) works correctly;
+ positive 'base' (digital) number values work correctly;
+ negative base (digital) number values work correctly;
+ base string values (in quotes) work correctly;
+ brackets `(`, `)` with a basic bool   value work correctly;
+ brackets `(`, `)` with a basic number value work correctly;
+ brackets `(`, `)` with a basic string value work correctly;
+ bool   fields work correctly;
+ number fields work correctly;
+ string fields work correctly;

+ Comparsion   `>`  works correctly with two     basic number values;
+ Comparsion   `>=` works correctly with two     basic number values;
+ Comparsion   `<`  works correctly with two     basic number values;
+ Comparsion   `<=` works correctly with two     basic number values;
+ EQUALITY     `=`  works correctly with two     basic bool   values;
+ EQUALITY     `=`  works correctly with several basic bool   values;
+ EQUALITY     `=`  works correctly with two     basic number values;
+ EQUALITY     `=`  works correctly with several basic number values;
+ EQUALITY     `=`  works correctly with two     basic string values;
+ EQUALITY     `=`  works correctly with several basic string values;
+ NOT-EQUALITY `!=` works correctly with two     basic bool   values;
+ NOT-EQUALITY `!=` works correctly with several basic bool   values;
+ NOT-EQUALITY `!=` works correctly with two     basic number values;
+ NOT-EQUALITY `!=` works correctly with several basic number values;
+ NOT-EQUALITY `!=` works correctly with two     basic string values;
+ NOT-EQUALITY `!=` works correctly with several basic string values;
+ AND          `&`  works correctly with two     basic bool   values;
+ AND          `&`  works correctly with several basic bool   values;
+ OR           `|`  works correctly with two     basic bool   values;
+ OR           `|`  works correctly with several basic bool   values;

+ COMPOSITION  `*`  works correctly with two     basic number values;
+ COMPOSITION  `*`  works correctly with several basic number values;
+ DIVISION     `*`  works correctly with two     basic number values;
+ DIVISION     `*`  works correctly with several basic number values;
+ SUM          `+`  works correctly with two     basic number values;
+ SUM          `+`  works correctly with several basic number values;
+ DIFFERENCE   `-`  works correctly with two     basic number values;
+ DIFFERENCE   `-`  works correctly with several basic number values;

+ JOIN         `+`  works correctly with two     basic string values;
+ JOIN         `+`  works correctly with several basic string values;

+ function CONTAINS works correctly with two   basic arguments values;
+ function IF       works correctly with two   basic argument for numbers;
+ function IF       works correctly with three basic arguments for numbers;
+ function IF       works correctly with two   basic argument  for strings;
+ function IF       works correctly with three basic arguments for strings;
+ function TOSTRING works correctly with any   basic argument;

+ negative for any bool   values (`!`) works correctly;
+ negation for any number values (`-`) works correctly;

+ ALL of the things above work correctly with non-basic values - other operators and functions
+ order of operations works correctly;

+ EMPTYFUNCTION works correctly with any argument;
+ format works correctly;

## III. Other testings
**Check:**
+ readme.md structure must be verified;
+ all readme.md examples must be tested in real;
