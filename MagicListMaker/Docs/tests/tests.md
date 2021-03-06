# Testing the program
Throughout the whole test the error messages must be tracked. They must be SHORT, CORRECT, COMPLETE and CLEAR for understanding. If not - this should be fixed.

## I. Synthax testings
### Database declaration testings
**Check:**
+ works correctly if declaration is missing, but gives an error when try to make a list without a declaration;
+ an error if declaration close tag is missing; `01`
+ an error if declaration is empty; `02`
+ works correctly if declaration has one database; `03`
+ works correctly if declaration has several databases; `04`
+ the path values are valid only when they are in quotes; `05 - 07`
+ works correctly with several declarations; `08`
+ an error when trying to declare a base with the name that is already declared; `9`
+ an error when trying to declare a base with the path that is already declared; `10`

### List declaration testings
**Check:**
+ works correctly if declaration is missing;
+ an error if declaration close tag is missing; `01`
+ an error if databases are missing; `02`
+ works correctly without options; `03`
+ works correctly with one option; `04`
+ works correctly with several options; `05`
+ an error if one option is declared several times; `06`
+ works correctly if declaration has one database; `04`
+ works correctly if declaration has several databases; `07`
+ works correctly if there are several list declarations; `08`
+ list base synthax works correctly, and the program responds an error if there is a base synthax error;

### Base value testings
**Check:**
+ for databases:
  - works correctly if only one database is used; `01`
  - works correctly if several databases are used; `02`
  - an error if an empty string is used in databases; `03`
  - an error if a non-declared database is used; `04`
  - works correctly if some symbols in the names are in wrong register; `05`
+ for options:
  - all bool values are valid only when `true`, `false`, `0`, or `1` is set as a value, quotes are not applied; `06 - 08`
  - all number values are valid only when an integer or a number is set as a value, quotes are not applied; `09` `10`
  - all string values are valid only when the value is in single quotes; `11 - 13`
+ for filter:
  - the value is valid only in single quotes; `14` `15`
  - an error if the value is not a bool; `16` `17`
+ for grouping:
  - the value is valid only in single quotes; `18` `19`
  - an error if sorting is missing; `20`
  - an error if the value is not a set of field names enumeration, separeted by `,`; `21 - 24` `29` `30`
+ for sorting:
  - the value is valid only in single quotes; `25` `26`
  - an error if the value is not a field names enumeration; `27`
+ for format:
  - the value is valid only in single quotes; `28`

## II. Functioning testings

### Database parsing testings
**Check:**
+ an error if the file doesn't exist; `01`
+ an error if the path doesn't exist; `02`
+ if the file and the path exist, but the file path is unusual:
  - works correctly with both `/` and `\` symbols; `03`
  - works correctly with double `/` or `\` symbol after drive letter; `04` `05`
  - works correctly with multi-letter drives; `missing`
  - works correctly if the file is on server (`\\Server\...` or `\\192.168...`); `06`
+ if the file exists, but the content is incorrect:
  - an error if the file is empty; `07`
  - an error if it has an incorrect first string (column string); `08` `09`
  - an error if it has an unregistered column; `08` `09`
  - an error if it has correct first line, but it's length doesn't match the length of other lines; `10`
  - an error if it has correct first line, but incorrect other document in any other way; `11`
+ if the file exists and it is correct:
  - works correctly if only one column is in input (use several different columns for that, including something very unusual); `12 - 14`
  - works correctly if every column is in input; `15`
  - works correctly with several columns as input; `16`
  - works correctly with missing grades; `16`
  - grades replacement works correctly; `16`
  - works correctly even if the whole MA Database is loaded; `15`
+ merge works correctly; `17`
+ several declarations works correctly; `18`

### Note parsing testings
**Check:**
+ works according to default if the parseNotes option isn't set; `01`
+ for `parseNotes = false` (`a`), `parseNotes = true` and a note was parsed (`b`), and `parseNotes = true` and no note was parsed (`c`), check the following:
  - for every-field-export (`every`) and for one-field-export (`one`):
    * every Magic Album field is written in appropriate card field; `02`
    * no field contains a null value; `02`
+ for `parseNotes = false`:
  - it separates foil and non-foil as usual; `03`
  - `comments` field contains `note` field info without changes; `04`
+ for `parseNotes = true`:
  - parse works with `,` and `;`; `05`
  - parse works with `.` in the end; `06`
  - works correctly when a parameter has a string like "NM/M with a stamp"; `07`
  - comments works correctly; `08`
  - for discount: `09`
    * `d10`, `d200` works correctly;
    * `10%`, `200%` works correctly;
    * `d-10`, `d-200` works correctly;
    * `-10%`, `-200%` works correctly;
  - for dollar rate:
    * `r50` works correctly; `10`
    * `r0.30` works correctly; `10`
    * an error for `r-50` or `r-0.30`; `11` `12`
  - any-field parameter works correctly; `13` - `16`
  - several any-field parameters work correctly; `17`
  - language parameter works correctly; `18` `19`
  - positive price works correctly; `20` `21`
  - an error when trying to set a negative price; `22`
  - priority works correctly; `23`
  - a parameter with all the options in any order works correctly; `24`
  - an error for unexpected parameters; `25`
  - it works correctly when parsing different types of parameters declared in any order; `26`
  - an error if sum of qtys in parameters is bigger than qty of cards; `27`
  - works correctly if sum of qtys in parameters is equal to qty of cards; `28`

### Price and round testings
**Check:**
+ defaultDollarRate works correctly; `01`
+ dollar rate in card field overrides defaultDollarRate; `02`
+ discount in card field overrides defaultDiscount and all default discounts for grades; `03`
+ in other cases, for graded cards, if default discount for these cards is not 0, default discounts for grades are used; `04`
+ in any other cases, default discount is used; `05`
+ smart round works correctly regardless of if round option is set or not; `06` `07`
+ minimum price works if it is different from 0; `08` `09`
+ if smartRound is off and round is 0, no round applies; `10`
+ if smartRound is off and round is different from 0, round works and does it correctly (with fractional numbers and integers as a parameter); `11` - `14`
+ all default options work according to default if the appropriate options aren't set; `15`

### Other options testings
**Check:**
+ works correctly when handleMultiNames is on; `01`
+ works correctly when handleMultiNames is off;
+ handleMultiNames works according to default if the handleMultiNames option isn't set; `03`

### Sorting and grouping testings
**Check:**
+ works correctly with one sort field; `01`
+ works correctly with one several sort fields; `02`
+ reverse sorting works correctly; `03`
+ works correctly with one group with one field; `04`
+ works correctly with one group with several field; `05`
+ works correctly with several groups; `06`

### Filter and formatting testings
**Check:**
+ `true` and `false` values work correctly; `001` - `003`
+ negative for `true` and `false` values (`!`) works correctly; `004` - `006`
+ positive 'base' (digital) number values work correctly; `007` - `009`
+ negative base (digital) number values work correctly; `010` `011`
+ base string values (in quotes) work correctly; `012` `019` `020`
+ brackets `(`, `)` with a basic bool   value work correctly; `013` `014` | `063`
+ brackets `(`, `)` with a basic number value work correctly; `015` `016` | `064`
+ brackets `(`, `)` with a basic string value work correctly; `017` `018` | `065`
+ bool   fields work correctly; `021`
+ number fields work correctly; `022`
+ string fields work correctly; `023`
+ works correctly if some symbols in the field names are in wrong register; `024`

+ Comparsion   `>`  works correctly with two     basic number values; `025` | `066`
+ Comparsion   `>=` works correctly with two     basic number values; `026` | `067`
+ Comparsion   `<`  works correctly with two     basic number values; `027` | `068`
+ Comparsion   `<=` works correctly with two     basic number values; `028` | `069`
+ EQUALITY     `=`  works correctly with two     basic bool   values; `029` | `070`
+ EQUALITY     `=`  works correctly with several basic bool   values; `030` | `071`
+ EQUALITY     `=`  works correctly with two     basic number values; `031` | `072`
+ EQUALITY     `=`  works correctly with several basic number values; `032` | `073`
+ EQUALITY     `=`  works correctly with two     basic string values; `033` | `074`
+ EQUALITY     `=`  works correctly with several basic string values; `034` | `075`
+ NOT-EQUALITY `!=` works correctly with two     basic bool   values; `035` | `076`
+ NOT-EQUALITY `!=` works correctly with several basic bool   values; `036` | `077`
+ NOT-EQUALITY `!=` works correctly with two     basic number values; `037` | `078`
+ NOT-EQUALITY `!=` works correctly with several basic number values; `038` | `079`
+ NOT-EQUALITY `!=` works correctly with two     basic string values; `039` | `080`
+ NOT-EQUALITY `!=` works correctly with several basic string values; `040` | `081`
+ AND          `&`  works correctly with two     basic bool   values; `041` | `082`
+ AND          `&`  works correctly with several basic bool   values; `042` | `083`
+ OR           `|`  works correctly with two     basic bool   values; `043` | `085`
+ OR           `|`  works correctly with several basic bool   values; `044` | `086`

+ COMPOSITION  `*`  works correctly with two     basic number values; `045` | `087`
+ COMPOSITION  `*`  works correctly with several basic number values; `046` | `087`
+ DIVISION     `/`  works correctly with two     basic number values; `047` | `087`
+ DIVISION     `/`  works correctly with several basic number values; `048` | `087`
+ SUM          `+`  works correctly with two     basic number values; `049` | `087`
+ SUM          `+`  works correctly with several basic number values; `050` | `087`
+ DIFFERENCE   `-`  works correctly with two     basic number values; `051` | `087`
+ DIFFERENCE   `-`  works correctly with several basic number values; `052` | `087`

+ JOIN         `+`  works correctly with two     basic string values; `053` | `088`
+ JOIN         `+`  works correctly with several basic string values; `054` | `088`

+ function CONTAINS works correctly with two   basic arguments values; `055` | `089`
+ function IF       works correctly with two   basic argument for numbers; `056` | `090`
+ function IF       works correctly with three basic arguments for numbers; `057` | `091`
+ function IF       works correctly with two   basic argument  for strings; `058` | `092`
+ function IF       works correctly with three basic arguments for strings; `059` | `093`
+ function TOSTRING works correctly with any   basic argument; `060` | `094`

+ negative for any bool   values (`!`) works correctly; `061` | `095`
+ negation for any number values (`-`) works correctly; `045` - `052` `062`

+ ALL of the things above work correctly with non-basic values - other operators and functions `063` - `095`
+ order of operations works correctly; `087` `096`

+ EMPTYFUNCTION works correctly with any argument; `almost all the tests`
+ format works correctly; `all the tests`

## III. Other testings
**Check:**
+ readme.md structure must be verified;
+ all readme.md examples must be tested in real;
