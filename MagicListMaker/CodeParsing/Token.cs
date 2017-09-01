using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MagicParser.CodeParsing
{
    public class Token
    {
        public string name;
        public bool isOption;
        public string optionType;
        public FieldInfo field;

        public Token (string name,  bool isOption = false, string optionType = null, FieldInfo field = null)
        {
            this.name = name;
            this.isOption = isOption;
            this.optionType = optionType;
            this.field = field;
        }

        public static string declarationBeginToken       = "dbs";
        public static string declarationEndToken         = "enddbs";
        public static string listBeginToken              = "list";
        public static string listEndToken                = "endlist";

        public static string databasesToken              = "dbs";

        public static string parseNotesToken             = "parseNotes";
        public static string defaultDollarRateToken      = "defaultDollarRate";
        public static string defaultDiscountToken        = "defaultDiscount";
        public static string defaultGemMintDiscountToken = "defaultGemMintDiscount";
        public static string defaultMintDiscountToken    = "defaultMintDiscount";
        public static string defaultNMDiscountToken      = "defaultNMMDiscount";
        public static string defaultNMMDiscountToken     = "defaultNMDiscount";
        public static string defaultNMSPDiscountToken    = "defaultNMSPDiscount";
        public static string defaultSPDiscountToken      = "defaultSPDiscount";
        public static string defaultSPMPDiscountToken    = "defaultSPMPDiscount";
        public static string defaultMPDiscountToken      = "defaultMPDiscount";
        public static string defaultMPHPDiscountToken    = "defaultMPHPDiscount";
        public static string defaultHPDiscountToken      = "defaultHPDiscount";
        public static string smartRoundToken             = "smartRound";
        public static string roundToken                  = "round";
        public static string handleMultiNamesToken       = "handleMultiNames";

        public static string filterToken                 = "filter";
        public static string groupingToken               = "group";
        public static string sortingToken                = "sort";
        public static string formattingToken             = "format";
    }
}
