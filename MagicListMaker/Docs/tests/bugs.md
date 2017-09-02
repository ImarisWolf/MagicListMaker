# Testing the program
Throughout the whole test the error messages must be tracked. They must be SHORT, CORRECT, COMPLETE and CLEAR for understanding. If not - this should be fixed.

## I. Synthax testings
### Database declaration testings
+ an error when trying to declare a base with the path that is already declared; `10`
**Nope.**

## II. Functioning testings

### Database parsing testings
**Check:**
+ if the file and path exist, but the file path is unusual:
  - works correctly with multi-letter drives; `missing`
+ if the file exists and it is correct:
  - works correctly if every column is in input; `15`
**Nope with 15, but generally it works fine.**
  - works correctly even if the whole MA Database is loaded; `15`
**Nope. Too long.**

### Sorting and grouping testings
**Check:**
+ works correctly with several groups; `06`
**Nope.**
