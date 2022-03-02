LocaleTools - LTools information Read Me


LocaleTools is a helper library which provides static functions to obtain localized versions of 
various strings.

These are primarily drawn from the database but also other associated miscellaneous strings which 
may be needed globally within the solution.

A standardized method is used to determine the resource id and so it is possible to use the English 
text as the key and derive any available localized text.

The library is also used within the XMLprocessor command line tool which can extract new strings 
from the database and automatically prepare pseudo translations of these in the resource files to 
facilitate easy maintenance of new database strings. 

The LocaleTools library contains these resource files.

Additionally the library provides an interface to an ICU Message Format library which is used for 
plural expressions. ICU Message Format is a well known industry standard.

Documentation for ICU message formatting is here:

http://userguide.icu-project.org/formatparse/messages

Example retrieving plural string:

LTools.Format(My.Resources.AuthorisationDetails_plural_will_expire_within_days, "COUNT", daysToLast)

Translators must take care when translating plural expressions within the resource files. Typically 
these will have _plural within the resource ID. 

Depending on the locale, different grammar rules may apply. These need to be adequately represented 
for the locale and care must be taken not to corrupt the variables and braces {}, commas and the # 
symbol within the expression. Keywords like "plural" "one" "two" "few" "many" "other" should not be 
translated.

"All license keys will expire within {COUNT, plural, one {1 day} other {# days}}. Please contact 
Blue Prism."

Translatable text in the above is marked with x's below:

"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx {COUNT, plural, one {x xxx} other {# xxxx}}x 
xxxxxxxxxxxxxxxxxxxxxxxxx"


LocaleTools also provides a central location to manage ordinals. These require both an ordinal type 
and ordinal text. 

If a language is added which needs ordinal type handling more complex than the generic "other" 
type. The rules for this need to be added in the GetOrdTypeEnum function. (English is already 
supported)

More detail on the logic for the Rules is described in the associated column of:

http://www.unicode.org/cldr/charts/latest/supplemental/language_plural_rules.html


Example using LTools to retrieve a string:

LTools.GetC(myString, "misc", "status"))

Retrieves the resource status_abc123 from the misc resource file for the current locale. where 
"abc123" is the content of the myString variable.
