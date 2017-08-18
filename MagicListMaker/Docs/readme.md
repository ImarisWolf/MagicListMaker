# What is it?
A program for making lists of Magic: the Gathering cards using [Magic Album](https://www.slightlymagic.net/wiki/Magic_Album). You need to export your MA inventory to TXT and load it in Magic Parser. Magic Parser parses it and makes a new list that can be filtered, sorted, grouped and formatted using special inner language.

I've made it just for myself and strictly for my special needs, but if you want to use it - of course you may `git clone` and compile it.

## Why do I need it at all?
It all started with a need to make a trade topic with all my cards. I have thousands of them, so it's not an easy affair to build the topic by hands. The Magic Album doesn't provide a readable card list export, so I decided to do it on my own.

## What are these "special needs"?
For example, the main need. I have cards of different conditions, or grades (NM, SP...), in my inventory, and it is important to make difference between them. But Magic Album doesn't allow to separate two identical cards by grades. One name, one set, one language - this forms strictly one field with any quantity of cards, but the _grade_ field is one for any quantity. So, if I have four identical cards, on of them is NM, two SP and one MP, I can't even set the grade for free text. So I write the different grades into the _note_ field, and Magic Parser parses the _note_, creating different field for every really unique card. I can also set different prices in the same way.

The other need - some work with prices. In my country people are used to take prices from one foreign site, and the prices are in dollars, but traders can set their own dollar rate for all cards and special rates for some certain cards. So, I want to be able to quickly convert the price in dollars into price in my currency using dollar rate that I can change at any time or set an unique rate to any card. The program allows to do it.

## How it works?
Firstly, you export the Magic Album Inventory as TXT file. Secondly, you load the file into Magic Parser. Then you write some text with some code. The text will be shown as is, and the code will be transformed into list of cards. In the code you choose a filter using logical expressions, sorting fields, group fields that are used while sorting, and a format line, which sets the template for every line.

## The language
_BNF for the language is in `language.ebnf`.
All tokens are case-insensitive.
Whitespaces are ignored, unless they are in quotes._

In the texbox you can write anything you want. In the output it will appear without changes.
The code can be of two types:
- database declaration;
- list.

Database declaration starts with `dbs` token and ends with `enddbs` token. Between these must be only declarations in form of:
`db_inner_name = 'full_path_to_db'`
You can declare several databases to use different exports in one file.

List starts with `list` token and ends with `endlist` token. Between there can be (strictly in the following format):
- databases to use in this list: `dbs = 'kithkins'`;
- parseComments option (parse the note or note - see above): `parseComments = false`;
- filter: `filter = 'bool expression'`;
- grouping: `group = 'nameoracle'`;
- sorting: `sort = '!price nameOracle grade'`;
- formatting: `format = 'format expression'`

The expressions are:
- And: `bool & bool`;
- Or: `bool | bool`;
- Equality: `bool = bool`, `number = number` or `string = string`; the same is for `!=` (not equal);
- comparsion: `number > number`, the same is for `<=`, `>`, `>=`;
- not: `!bool`;
- sum: `number + number`, `number - number` or `string + string`;
- composition: `number * number`, the same is for `/`;
- field: `$fieldName`;
- brackets: `(anyValue)`
- if: `if(bool, number[, number])` or `if(bool, string[, string])`. If first argument is true, return second argument, else return third argument (if present) or 0 or "".
- contains: `contains(string, string)`. If first string contains second - return true, else return false.
- toString: `toString(anyValue)` returns bool as string ("true" and "false") and number as string.

## The note language:
_BNF for the note language is in `note.ebnf`._
If the parseComments option isn't set to 'false', Magic Parser will parse the note using special rules.
You can set any number of these parameters into the note:
- grade: `NM`, `Mint`, `SP+` and so on;
- price: just a number (in dollars);
- language: `ENG` or `English`, `Spa`, `Spanish`, etc.;
- dollarRate: `c40` or `r40` for setting dollar rate for this card to 40 (40 units for one dollar);
- discount: `d10` or `10%` for discount 10% for this card. Extra charge 5% can be made this way: `d-5` or `-5%`;
- comment: any text in quotes, "like this". This can be used instead of a normal note.
- priority: `p1`, `p2`... This can help you to group you cards with some priorities, for example, for making a wishlist;
- field: `$name` or `$set` - use it if you want to change any origin field of a card.

Parameters are separated by `,` or `;`.
Each parameter can have _qty_ and _type_ option that are placed before it's name. The _type_ can be `foil` or `nonfoil` (or `non-foil`). The _qty_ is a number of cards to which the parameter applies.
For example: `2 SP`, `1 foil ENG`, `foil SP`.
If _qty_ is missing - the parameter is applied to all cards of the chosen type. For example: `foil JAP`.
If _type_ is minning, the parameter is applied to all types.
**Firstly**, any parameter that is applied to all cards, redefines the original fields.
**Then**, any parameter that is applied to all cards of a certain type, redefines the fields of those cards.
**Then**, any parameter that is applied to a certain number of cards, redefines the fields of those cards.
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

You can also set multi-parameter. For example: `2 non-foil SP Korean`, or `3 NM, 2 SP d20, 1 MP d40`.
