# What is it?
Magic List Maker is a program for making lists of Magic: the Gathering cards using [Magic Album](https://www.slightlymagic.net/wiki/Magic_Album) inventories. You need to export your MA inventory to TXT and load it in Magic List Maker. Magic List Maker parses it and makes a new list that can be filtered, sorted, grouped and formatted using special inner language.

I've made it just for myself and strictly for my special needs, but if you want to use it - of course you may `git clone` and compile it.

## Why do I need it at all?
It all started with a need to make a trade topic for [Topdeck.ru](http://topdeck.ru/forum/) with all my cards to trade. I have thousands of them, so it's not an easy affair to build the topic by hands. The Magic Album doesn't provide a readable card list export, so I decided to make one on my own.

## What are these "special needs"?
I have cards of different conditions, or grades in MA terminology (NM, SP...), in my inventory, and it is important to make difference between them. But Magic Album doesn't allow to separate two identical cards by grades. One name, one set, one language (and one version - e.g. Urza lands from Antiquities) - this forms strictly one field with any quantity of cards, but the _Grade_ field is one for any quantity. So, if I have four identical cards, on of them is NM, two SP and one MP, I can't even set the grade for free text. So I write the different grades into the _Note_ field, and Magic List Maker parses the _Note_, creating different line in the list of cards for every really unique card. I can also set different prices in the same way.

The other need - some work with prices. In my country people are used to take prices from one foreign site, and the prices are in dollars, but traders can set their own dollar rate for cards they sell and special rates for some certain cards. So, I want to be able to quickly convert the price in dollars into price in my currency using dollar rate that I can change at any time, or set an unique rate to any card. The program allows to do it.
Also, MA doesn't make difference between price for foil card and non-foil. So, I am used to write prices for non-foil cards in _Sell Price_ field, and for foil cards - in _Buy Price_ field (I don't use it anyway).

## How it works?
Firstly, you export the Magic Album Inventory as TXT file. Secondly, you load the file into Magic List Maker. Then you write some text with some code. The text will be shown as is, and the code will be transformed into list of cards. In the code you choose a filter using logical expressions, then you choose group fields that are used while sorting, sorting fields, and a format line, which sets the template for every line.

## The language
_BNF for the language is in `MagicListMaker/Docs/language.ebnf`.
All tokens are case-insensitive.
Whitespaces are ignored, unless they are in quotes._

In the texbox you can write anything you want. In the output it will appear without changes.
The code can be of two types:
+ database declaration;
+ list.

Database declaration starts with `dbs` token and ends with `enddbs` token. Between these must be only declarations in form of:
`db_inner_name = 'full_path_to_db'`
You can declare several databases to use different exports in one file.
Without DBs declaration you won't be able to use databases in lists.

List starts with `list` token and ends with `endlist` token. Between there can be (strictly in the following format):
+ databases to use in this list: `dbs = 'db_inner_name'`;
+ options in any order, each option looks like this: `option_name = option_value`;
+ filter: `filter = 'bool_expression'`;
+ grouping: `group = 'group1_field1 group1_field2 group1_field3, group2_field1 group2_field2'`;
+ sorting: `sort = 'first_field_to_sort, second, !third_in_reverse_order'`;
+ formatting: `format = 'format_expression'`

Databases you use must be declared above.

### The options
The options are:
+ `parseComments`, bool, `true` is default. Enables the note parsing (for separete cards by grades and other fields).
+ `defaultDollarRate`, number, `40` is default. `0` means make no conversion. Sets dollar rate that is applied to all cards by default;
+ `defaultDiscount`, number. Sets discounts for cards of different grades that are applied to these cards by default;
  - `defaultDiscount`        - for all cards.      Default = 0;
  - `defaultGemMintDiscount` - for Gem Mint cards. Default = 0;
  - `defaultMintDiscount`    - for Mint cards.     Default = 0;
  - `defaultNMMDiscount`     - for NM/M cards.     Default = 0;
  - `defaultNMDiscount`      - for NM cards.       Default = 0;
  - `defaultNMSPDiscount`    - for NM/SP cards.    Default = 5;
  - `defaultSPDiscount`      - for SP cards.       Default = 15;
  - `defaultSPMPDiscount`    - for SP/MP cards.    Default = 20;
  - `defaultMPDiscount`      - for MP cards.       Default = 30;
  - `defaultMPHPDiscount`    - for MP/HP cards.    Default = 40;
  - `defaultHPDiscount`;     - for HP cards.       Default = 50;
+ `smartRound`, bool, `true` is default. Enables beautiful round for all prices;
+ `round`, number, `1` is default. `0` means make no round. Rounds prices to a multiple of value;
+ `handleMultiNames`, bool, `true` is default. If enabled, double-faced and flip cards are parsed as always, and for split cards '|' symbols are removed.

**Smart Round principle.** At first, the price is rounded up to an integer. Then, if the price have _n_ digits, the last _n-3_ digits are rounded up to 0, the trird - up to 0 or 5, and the first two are not rounded. E.g., 67743,11 will be rounded up to 67500, 191,43 - to 190, 11,54 - to 11.

### Filter
Filter uses logical expressions. For each card, if it satisfies the expression - it will appear in the list. Usually you just need to use `$fieldName` to compare it with something, but the language provides more powerful filter. There are logical and arithmetical operators and some functions with strings:
+ And: `bool & bool`, returns bool;
+ Or: `bool | bool`, returns bool;
+ Equality: `bool = bool`, `number = number` or `string = string`; the same is for `!=` (not equal); returns bool;
+ Comparsion: `number > number`, the same is for `>=`, `<`, `<=`; returns bool;
+ not: `!bool`; returns bool;
+ sum: `number + number` or `number - number`, returns number;
+ composition: `number * number`, the same is for `/`; returns number;
+ field: `$fieldName`, returns bool, number or string - depending on field;
+ if: `if(bool, number[, number])` or `if(bool, string[, string])`. If first argument is true, returns second argument, else returns third argument (if present) or 0 (if number) or "" (if string).
+ contains: `contains(string, string)`. If first argument contains second - returns true, else returns false.
+ join: `string + string`, returns string;
+ toString: `toString(anyValue)` returns bool as string ("true" and "false") and number as string.
Also you can use brackets: `(`, `)`.
Any text in double quotes (`"`) counts as a string. Number counts as a number (use dot `.` to separate fraction), `true` and `false` counts as relevant bool values.
There is no escape symbol in the language at the moment.
You can combine all the operators as you want.

### Grouping
Grouping can be set when sorting is used. Guess you have a list:
+ Cloudstone Curio - NM - $9.50
+ Forced Fruition - NM - $10.00
+ Forced Fruition - SP - $8.00
+ Nissa, Steward of Elements - NM - $9.00
And you have sorted the list by price from highest to lowest:
+ Forced Fruition - NM - $10.00
+ Cloudstone Curio - NM - $9.50
+ Nissa, Steward of Elements - NM - $9.00
+ Forced Fruition - SP - $8.00
Now you look and see that two cards with one name are separated by several lines. And you'd rather want something like that:
+ Forced Fruition - NM - $10.00
+ Forced Fruition - SP - $8.00
+ Cloudstone Curio - NM - $9.50
+ Nissa, Steward of Elements - NM - $9.00
With just sorting you can't do it. Sorting by nameat first priorty will crush you sorting by price:
+ Cloudstone Curio - NM - $9.50
+ Forced Fruition - NM - $10.00
+ Forced Fruition - SP - $8.00
+ Nissa, Steward of Elements - NM - $9.00
And sorting by name as a second priority will sort by names only the cards with equal price. But grouping can do it. Firstly, the list is sorted by price in reverse order, then it is grouped by name.
You can also group by several fields, e.g., by name and language. Guess we have sorted by price list:
+ Forced Fruition - ENG - NM - $10.00
+ Cloudstone Curio ENG - NM - $9.50
+ Forced Fruition - JAP - SP+ - $9.00
+ Nissa, Steward of Elements - ENG - NM - $9.00
+ Forced Fruition - ENG - SP - $8.00
`group = 'nameOracle language'` will reorder the list this way:
+ Forced Fruition - ENG - NM - $10.00
+ Forced Fruition - ENG - SP - $8.00
+ Cloudstone Curio ENG - NM - $9.50
+ Forced Fruition - JAP - SP+ - $9.00
+ Nissa, Steward of Elements - ENG - NM - $9.00
Because JAP Forced Fruition is not in the "Forced Fruition ENG" group.

You can also make several groups: `group = 'nameOracle language, nameOracle grade'`. Firstly it will group together all cards with the same nameOracle and language, then it will search the remaining ungrouped cards for cards with the same nameOracle and grade, and group them.

### Sorting
To set sorting, just write the sorted values in sort priority order: `sort = 'nameOracle grade language'`. So, any cards with equivalent nameOracle will be sorted by grade, and if nameOracle and grade are the same - the cards will be sorted by language.
By default the program sorts from lowest to highest. To sort in reverse order you need to use `!` symbol before the field name: `sort = '!price'`.

### Formatting
Using this option you set a pattern for each card (for each line). E.g.: `format = '$name - $price'`. This transforms into:
+ Horde of Notions - 0.69
+ Primal Beyond - 2.49
+ etc.
As you can see, you can use all the same operators that you were used in filter. There're also one more operator - "empty fumction", that is written as `$(anyValue)`. All text in this function and other functions which begin with `$` are parsed as operators, all other text - just a text. E.g.: `$qty + $price` will return something like that: `1 + 10.00`, and `$($qty + $price)` will return `11.00`.

## The note language:
_BNF for the note language is in `MagicListMaker/Docs/note.ebnf`._
If the parseComments option isn't set to 'false', Magic List Maker will parse the note using special rules.
You can write any number of parameters that are are separated by `,` or `;` into the note.
Each parameter can hold any number of these options:
+ grade: `NM`, `Mint`, `SP/MP` and so on;
+ price: just a number;
+ language: `ENG` or `English`, `Spa`, `Spanish`, etc.;
+ dollarRate: `c40` or `r40` for setting special dollar rate for this card to 40 (40 units for one dollar);
+ discount: `d10` or `10%` for discount 10% for this card. Extra charge 5% can be made this way: `d-5` or `-5%`;
+ comment: any text in quotes, "like this". This can be used instead of a normal note.
+ priority: `p1`, `p2`... This can help you to group you cards with some priorities, for example, for making a wishlist;
+ field: `$name` or `$set` - use it if you want to change any origin field of a card.

Grade can contain something like `NM/M with a stamp`, the parser will automatically write the whole text in a grade, but if it faces any word that appers to be language parameter, dollar rate parameter or any other, the program can work incorrectly or respond an error. Also, for this example, the default discount will be as for NM/M card, and if you want to inherit SP or MP discount for NM card with a stamp, you need to place the condition you want to the beginning of the parameter, e.g.: `MP (NM/M with a stamp)`.

Each parameter can also haveone  _qty_ and one _type_ option that are placed before other options. The _type_ can be `foil` or `non-foil` (or `nonfoil`). The _qty_ is a number of cards to which the parameter applies.
For example: `2 SP`, `1 foil ENG`, `foil SP`.
If _qty_ is missing - the parameter is applied to all cards of the chosen type. E.g.: `foil JAP` - all foil cards in the current position are now Japanese.
If _type_ is minning, the parameter is applied to all types.
**Firstly**, any parameter that is applied to all cards, redefines the original fields.
**Then**, any parameter that is applied to all cards of a certain type, redefines the fields of those cards.
**Then**, any parameter that is applied to a certain number of cards, redefines the fields of **non-foil** cards.
**Then**, any parameter that is applied to a certain number of cards of a certain type, redefines the fields of those cards.

So, guess we have a card. The original grade for regular cards is set to NM/M, for foil - to SP. Quantity of regular cards is 4, foil - 5.
The note has: `Mint, foil SP+, 2 non-foil SP, 1 foil SP, 1 foil MP`.
Now instead of:
+ 4x non-foil NM/M
+ 5x foil SP
we have:
+ 2x non-foil Mint
+ 2x non-foil SP
+ 3x foil SP+
+ 1x foil SP
+ 1x foil MP

You can also set multi-option parameter. For example: `2 non-foil SP Korean`, or `3 NM, 2 SP 20%, 1 MP 40%`.
