using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static MagicParser.Database;

namespace MagicParser.CodeParsing
{
    public class Analizer
    {
        #region Fields
        public string input { get; private set; } //текст для разбора
        private string token { get; set; } //Текущий токен - для удобства доступа в своём поле
        
        private Dictionary<string, Database> dbs { get; set; } //Список баз данных по типу 'внутреннее имя БД' - 'ссылка на БД'
        public string errorDescription { get; private set; } //Сюда записывается описание ошибки; после вызова каждого метода обязательно нужно делать проверку, нет ли здесь информации, если есть - прекращать работу анализатора
        public List<Token> tokens { get; set; }
        public static int tokenizerLastErrorPos = 0; //Последняя позиция токенайзера в случае, когда парсер вернул ошибку
        #endregion

        //Конструктор
        public Analizer(string input)
        {
            this.input = input;
            token = null;
            dbs = new Dictionary<string, Database>();
            errorDescription = null;

            tokens = new List<Token>();

            tokens.Add(new Token(Token.declarationBeginToken));
            tokens.Add(new Token(Token.declarationEndToken));
            tokens.Add(new Token(Token.listBeginToken));
            tokens.Add(new Token(Token.listEndToken));

            tokens.Add(new Token(Token.databasesToken));

            tokens.Add(new Token(Token.parseNotesToken,             true, "bool",   typeof(Database).GetField("parseNotes")));
            tokens.Add(new Token(Token.defaultDollarRateToken,      true, "number", typeof(Database).GetField("defaultDollarRate")));
            tokens.Add(new Token(Token.defaultDiscountToken,        true, "number", typeof(Database).GetField("defaultDiscount")));
            tokens.Add(new Token(Token.defaultGemMintDiscountToken, true, "number", typeof(Database).GetField("defaultGemMintDiscount")));
            tokens.Add(new Token(Token.defaultMintDiscountToken,    true, "number", typeof(Database).GetField("defaultMintDiscount")));
            tokens.Add(new Token(Token.defaultNMDiscountToken,      true, "number", typeof(Database).GetField("defaultNMMDiscount")));
            tokens.Add(new Token(Token.defaultNMMDiscountToken,     true, "number", typeof(Database).GetField("defaultNMDiscount")));
            tokens.Add(new Token(Token.defaultNMSPDiscountToken,    true, "number", typeof(Database).GetField("defaultNMSPDiscount")));
            tokens.Add(new Token(Token.defaultSPDiscountToken,      true, "number", typeof(Database).GetField("defaultSPDiscount")));
            tokens.Add(new Token(Token.defaultSPMPDiscountToken,    true, "number", typeof(Database).GetField("defaultSPMPDiscount")));
            tokens.Add(new Token(Token.defaultMPDiscountToken,      true, "number", typeof(Database).GetField("defaultMPDiscount")));
            tokens.Add(new Token(Token.defaultMPHPDiscountToken,    true, "number", typeof(Database).GetField("defaultMPHPDiscount")));
            tokens.Add(new Token(Token.defaultHPDiscountToken,      true, "number", typeof(Database).GetField("defaultHPDiscount")));
            tokens.Add(new Token(Token.smartRoundToken,             true, "bool",   typeof(Database).GetField("smartRound")));
            tokens.Add(new Token(Token.roundToken,                  true, "number", typeof(Database).GetField("round")));
            tokens.Add(new Token(Token.minimumPriceToken,           true, "number", typeof(Database).GetField("minimumPrice")));
            tokens.Add(new Token(Token.handleMultiNamesToken,       true, "bool",   typeof(Database).GetField("handleMultiNames")));

            tokens.Add(new Token(Token.filterToken));
            tokens.Add(new Token(Token.groupingToken));
            tokens.Add(new Token(Token.sortingToken));
            tokens.Add(new Token(Token.formattingToken));
        }


        #region General methods
        
        //получаем токен
        private void GetToken(Tokenizer t)
        {
            token = t.GetToken();
        }

        //Следующий токен - получается без изменения позиции токенайзера
        private string GetNextToken(Tokenizer t)
        {
            return t.ForseeToken();
        }

        //Шорткаты для ошибок
        private string ErrorExpected(string expected = "", bool quotes = true)
        {
            string output = "";
            string space = " ";
            if (token != "")
            {
                if (token != "'") output = "Unexpected token: '" + token + "'.";
                else output = "Unexpected token: \"'\".";
            }
            else space = "";

            if (expected != "")
            {
                if (quotes) output += space + "'" + expected + "' expected.";
                else output += space + expected + " expected.";
            }
            return output;
        }

        private string ErrorQuotes(string whatToDo)
        {
            return "Unexpected token: " + token + ". You must " + whatToDo + " in single quotes.";
        }

        #endregion


        //основной метод. Возвращает готовую строку с данными. Все другие методы сделаны в соответствии с BNF.
        //input = ([databasesDeclaration] [list] [freeText])*
        public string Parse()
        {
            Tokenizer t = new Tokenizer(input);
            string output = "";
            int lastPos = 0;
            //пока токенайзер не просмотрит весь инпут, выполняем
            while (!t.endIsReached)
            {
                //databasesDeclaration = declarationBeginToken declaration* declarationEndToken
                if (GetNextToken(t).ToLower() == Token.declarationBeginToken.ToLower())
                {
                    output += t.GetWhiteSpaces();
                    GetToken(t);
                    DBDeclarations(t);
                    if (errorDescription != null) { tokenizerLastErrorPos = t.pos; return output; }
                    GetToken(t);
                    if (token.ToLower() != Token.declarationEndToken.ToLower()) { errorDescription = ErrorExpected(Token.declarationEndToken); tokenizerLastErrorPos = t.pos; return output; }
                    lastPos = t.pos;
                }
                //list = listBeginToken listParams listEndToken
                else if (GetNextToken(t).ToLower() == Token.listBeginToken.ToLower())
                {
                    output += t.GetWhiteSpaces();
                    GetToken(t);
                    string result = ListParams(t);
                    if (errorDescription != null) { tokenizerLastErrorPos = t.pos; return output; }
                    output += result;
                    GetToken(t);
                    if (token.ToLower() != Token.listEndToken.ToLower()) { errorDescription = ErrorExpected(Token.listEndToken); tokenizerLastErrorPos = t.pos; return output; }
                    lastPos = t.pos;
                }
                //Иначе необходимо добавить весь текст (с пробелами) в аутпут
                //freeText = ?string that doesn't include codeBeginToken?
                else
                {
                    GetToken(t);
                    output += input.Substring(lastPos, t.pos - lastPos);
                    lastPos = t.pos;
                }
            }

            tokenizerLastErrorPos = 0;
            return output.TrimStart().TrimEnd();
        }

        //declaration*
        //declaration = name '=' "'" path "'";
        private void DBDeclarations(Tokenizer t)
        {
            do
            {
                //name = ?regexp?
                if (GetNextToken(t).ToLower() != Token.declarationEndToken.ToLower() && Regex.IsMatch(GetNextToken(t), @"^[a-zA-Z_]\w+$"))
                {
                    GetToken(t);
                    if (dbs.ContainsKey(token.ToLower()))
                    {
                        errorDescription = "Database with this name have been already declared. Choose another name.";
                        return;
                    }
                    string DBname = token.ToLower();
                    GetToken(t);
                    if (token == "=")
                    {
                        GetToken(t);
                        if (token == "'")
                        {
                            token = t.GetUntil('\'');
                            //path = ?regexp?;
                            if (Regex.IsMatch(token, @"^(?i)([a-z]+:)?[\\/]?([\\/].*)*[\\/]?(?-i)$"))
                            {
                                string DBpath = token;
                                if (!File.Exists(DBpath)) { errorDescription = "The file '" + DBpath + "' doesn't exist."; return; }
                                dbs.Add(DBname, new Database(DBpath));
                                GetToken(t);
                                if (token != "'") { errorDescription = ErrorQuotes("declare the path"); return; }
                            }
                            else { errorDescription = "Wrong file path: " + token; return; }
                        }
                        else { errorDescription = ErrorQuotes("declare the path"); return; }
                    }
                    else { errorDescription = ErrorExpected("="); return; }
                }
                else if (GetNextToken(t).ToLower() == "") { GetToken(t); errorDescription = ErrorExpected(Token.declarationEndToken); return; }
                else if (GetNextToken(t).ToLower() == Token.declarationEndToken.ToLower())
                {
                    if (dbs.Count() == 0) errorDescription = "At least one Database should me declared!";
                    return;
                }
                else { GetToken(t); errorDescription = "Wrong Database name: " + token; return; }
            }
            while (!t.endIsReached);
        }

        //listParams = databases [options] [filter] [[grouping] sorting] [formatting]
        private string ListParams(Tokenizer t)
        {
            List<Database> currentDBs = new List<Database>();
            GetToken(t);
            //databases = databasesToken '=' "'" dbsValue "'"
            if (token.ToLower() == Token.databasesToken.ToLower())
            {
                GetToken(t);
                if (token == "=")
                {
                    GetToken(t);
                    if (token == "'")
                    {
                        if (GetNextToken(t) == "'") { errorDescription = ErrorExpected("Databases names"); return ""; }
                        //dbsValue = name+
                        do
                        {
                            GetToken(t);
                            if (dbs.ContainsKey(token.ToLower()))
                            {
                                dbs[token.ToLower()].Clear();
                                currentDBs.Add(dbs[token.ToLower()]);
                            }
                            else { errorDescription = "Database with the name '" + token + "' doesn't exist. Declare it first."; return ""; }
                        }
                        while (!t.endIsReached && GetNextToken(t) != "'");
                        GetToken(t);
                        if (token != "'") { errorDescription = ErrorQuotes("define the names"); return ""; }
                    }
                    else { errorDescription = ErrorQuotes("define the names"); return ""; }
                }
                else { errorDescription = ErrorExpected("="); return ""; }
            }
            else { errorDescription = "You must choose databases you want to use in the listing."; return ""; }

            //[options] [filter] [[grouping] sorting] [formatting]
            //options = option+
            //option = boolOption | numberOption | stringOption
            //boolOption = boolOptionToken '=' bool
            //numberOption = numberOptionToken '=' number
            //stringOption = stringOptionToken '=' "'" ?string? "'"
            List<string> usedOptions = new List<string>();
            while (true)
            {
                bool noOptions = false;
                foreach (Token key in tokens)
                {
                    if (key.isOption && GetNextToken(t).ToLower() == key.name.ToLower())
                    {
                        noOptions = false;
                        if (!usedOptions.Contains(key.name.ToLower()))
                        {
                            HandleOption(t, key, currentDBs);
                            if (errorDescription != null) return "";
                            usedOptions.Add(key.name.ToLower());
                            break;
                        }
                        else { GetToken(t); errorDescription = "The option '" + token + "' has already been used"; return ""; }
                    }
                    else noOptions = true;
                }
                if (noOptions) break;
            }
            

            //После определения всех опций про парсинг парсим базы
            foreach (Database db in currentDBs)
            {
                db.ReadFile();
                if (db.errorDescription != null) { errorDescription = db.errorDescription; return ""; }
            }
            //Затем сливаем их
            Database mergedDB = Merge(currentDBs);

            //[filter]
            if (GetNextToken(t).ToLower() == Token.filterToken.ToLower())
            {
                Filter(t, mergedDB);
                if (errorDescription != null) return "";
            }

            //[grouping]
            List<List<string>> groupFields = new List<List<string>>();
            if (GetNextToken(t).ToLower() == Token.groupingToken.ToLower())
            {
                groupFields = Group(t);
                if (errorDescription != null) return "";
            }

            //[sorting]
            if (groupFields.Count != 0 && GetNextToken(t).ToLower() != Token.sortingToken.ToLower()) { GetToken(t); errorDescription = ErrorExpected(Token.sortingToken) + " If you set grouping, you must set sorting."; return ""; }
            if (GetNextToken(t).ToLower() == Token.sortingToken.ToLower())
            {
                if (groupFields.Count != 0) Sort(t, mergedDB, groupFields);
                else Sort(t, mergedDB);
                
                if (errorDescription != null) return "";
            }

            //[formatting]
            string output = "";
            if (GetNextToken(t).ToLower() == Token.formattingToken.ToLower())
            {
                output = Format(t, mergedDB);
                if (errorDescription != null) return output;
            }

            return output;
        }

        private void HandleOption(Tokenizer t, Token key, List<Database> currentDBs)
        {
            GetToken(t);
            GetToken(t);
            if (token == "=")
            {
                GetToken(t);
                if (key.optionType == "bool")
                {
                    bool value;

                    if (token.ToLower() == "true" || token == "1") value = true;
                    else if (token.ToLower() == "false" || token == "0") value = false;
                    else { errorDescription = ErrorExpected("Bool value", false); return; }
                    
                    foreach (Database db in currentDBs) key.field.SetValue(db, value);
                }
                else if (key.optionType == "number")
                {
                    float value;
                    if (!float.TryParse(token.Replace('.', ','), out value)) { errorDescription = ErrorExpected("Number value", false); return; }
                    foreach (Database db in currentDBs) key.field.SetValue(db, value);
                }
                else if (key.optionType == "string")
                {
                    //There are no string options by now
                }
                else { errorDescription = "Wrong token type parameter; fix the code. The option is: " + token; }
            }
            else { errorDescription = ErrorExpected("="); return; }
        }

        //filter = filterToken '=' "'" boolValue "'" ';'
        private void Filter(Tokenizer t, Database mergedDB)
        {
            GetToken(t);
            if (token == Token.filterToken.ToLower())
            {
                GetToken(t);
                if (token == "=")
                {
                    GetToken(t);
                    if (token == "'")
                    {
                        List<Entry> cardList = new List<Entry>(mergedDB.cardList);
                        int pos = t.pos;
                        foreach (Entry card in cardList)
                        {
                            Tokenizer T = new Tokenizer(t.input.Substring(pos));
                            bool delete = !BoolValue(T, card);
                            if (errorDescription != null) return;
                            if (delete) mergedDB.cardList.Remove(card);

                            GetToken(T);
                            if (token != "'") { errorDescription = ErrorQuotes("define the filter"); return; }

                            t.SetPos(T.pos + t.input.Length - T.input.Length);
                        }
                    }
                    else { errorDescription = ErrorQuotes("define the filter"); return; }
                }
                else { errorDescription = ErrorExpected("="); return; }
            }
            else { errorDescription = ErrorExpected(Token.filterToken); return; }


        }

        //grouping = groupingToken '=' "'" groupingValue "'"
        private List<List<string>> Group(Tokenizer t)
        {
            GetToken(t);
            if (token == Token.groupingToken.ToLower())
            {
                List<List<string>> groupFields = new List<List<string>>();
                GetToken(t);
                if (token == "=")
                {
                    //groupingValue = "'" field+ (',' field+)* "'"
                    GetToken(t);
                    if (token == "'")
                    {
                        if (GetNextToken(t) == "'") { errorDescription = ErrorExpected("Field name", false); return groupFields; }
                        //field+
                        //field = ?fieldName?
                        List<string> list = new List<string>();
                        do
                        {
                            GetToken(t);
                            //Проверяем существование поля с таким названием
                            FieldInfo field = typeof(Entry).GetField(token, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            if (field != null) list.Add(token.ToLower());
                            else { errorDescription = "Wrong field name: " + token; return null; }
                        }
                        while (!t.endIsReached && GetNextToken(t) != "," && GetNextToken(t) != "'");
                        groupFields.Add(list);
                        while (!t.endIsReached && GetNextToken(t) != "'")
                        {
                            if (GetNextToken(t) == ",")
                            {
                                GetToken(t); //','
                                List<string> l = new List<string>();
                                do
                                {
                                    GetToken(t);
                                    if (token == "'") { errorDescription = ErrorExpected("Database name", false); return null; }
                                    //Проверяем существование поля с таким названием
                                    FieldInfo field = typeof(Entry).GetField(token, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                                    if (field != null) l.Add(token.ToLower());
                                    else { errorDescription = "Wrong field name: " + token; return null; }
                                }
                                while (!t.endIsReached && GetNextToken(t) != "," && GetNextToken(t) != "'");
                            }
                            else if (GetNextToken(t) != "'") { GetToken(t); errorDescription = ErrorExpected("'"); return null; }
                            groupFields.Add(list);
                        }
                        GetToken(t);
                        if (token != "'") { errorDescription = ErrorExpected("'"); return null; }
                    }
                    else { errorDescription = ErrorQuotes("define the grouping"); return null; }
                }
                else { errorDescription = ErrorExpected("="); return null; }
                return groupFields;
            }
            else { errorDescription = ErrorExpected(Token.groupingToken); return null; }
        }

        //sorting = sortingToken '=' "'" sortingValue "'"
        private void Sort(Tokenizer t, Database db, List<List<string>> groupFields = null)
        {
            GetToken(t);
            if (token == Token.sortingToken.ToLower())
            {
                GetToken(t);
                if (token == "=")
                {
                    GetToken(t);
                    if (token == "'")
                    {
                        //sortingValue = (['!'] field)+;
                        List<Tuple<string, bool>> fields = new List<Tuple<string, bool>>();
                        do
                        {
                            bool directOrder = true;
                            if (GetNextToken(t) == "!")
                            {
                                GetToken(t);
                                directOrder = false;
                            }

                            GetToken(t);
                            //Проверяем существование поля с таким названием
                            FieldInfo field = typeof(Entry).GetField(token, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            if (field != null) fields.Add(new Tuple<string, bool>(token.ToLower(), directOrder));
                            else { errorDescription = "Wrong field name: " + token; return; }
                        }
                        while (!t.endIsReached && GetNextToken(t) != "'");

                        GetToken(t);
                        if (token != "'") { errorDescription = ErrorQuotes("define sorting"); return; }
                        
                        SortDB(db, fields);
                        if (errorDescription != null) return;
                        if (groupFields != null)
                        {
                            GiveGroupIDs(db, groupFields);
                            if (errorDescription != null) return;
                            fields.Insert(0, new Tuple<string, bool>("groupID", true));
                            SortDB(db, fields);
                            if (errorDescription != null) return;
                        }
                    }
                    else { errorDescription = ErrorQuotes("define sorting"); return; }
                }
                else { errorDescription = ErrorExpected("="); return; }
            }
            else { errorDescription = ErrorExpected(Token.sortingToken); return; }
        }

        //Выдаём айдишники для группировки отсортированному (!) списку карт. На вход подаётся список параметров группировки, каждый параметр является списком полей, по которым необходимо сгруппировать.
        private void GiveGroupIDs(Database sortedDB, List<List<string>> fieldNames)
        {
            List<Entry> cards = sortedDB.cardList;
            //Последний использованный айди
            int lastID = 0;
            //пробегаемся по каждому списку полей для группировки
            foreach (List<string> element in fieldNames)
            {
                //берём данные о реальных полях из имён и составляем список
                List<FieldInfo> fields = new List<FieldInfo>();
                foreach (string fieldName in element)
                {
                    FieldInfo field = typeof(Entry).GetField(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (field == null) { errorDescription = "Wrong value name: " + token; return; }
                    fields.Add(field);
                }

                //Проходимся по картам
                for (int i = 0; i < cards.Count; i++)
                {
                    //если карте айди не назначен - назначаем (иначе просто пропускаем)
                    if (cards[i].groupID == 0)
                    {
                        cards[i].groupID = lastID + 1;
                        lastID++;
                        //назначив айди, проходим по оставшемуся списку и в случае соответствия каждого поля приравняем айди
                        if (i + 1 < cards.Count)
                        {
                            for (int j = i + 1; j < cards.Count; j++)
                            {
                                bool addToGroup = true;
                                foreach (FieldInfo field in fields)
                                {
                                    object left = field.GetValue(cards[i]);
                                    object right = field.GetValue(cards[j]);
                                    if (!field.GetValue(cards[i]).Equals(field.GetValue(cards[j])))
                                    {
                                        addToGroup = false;
                                        break;
                                    }
                                }
                                if (addToGroup) cards[j].groupID = cards[i].groupID;
                            }
                        }
                    }
                }
            }
        }

        //Сортировка базы данных по указанным полям. В тупле содержится поле и порядок сортировки (true - прямой порядок, false - обратный)
        private void SortDB (Database db, List<Tuple<string, bool>> fieldNames)
        {
            db.cardList.Sort(delegate (Entry x, Entry y)
            {
                //Обходим филднеймы из списка по очереди, если по приоритетному параметру различаются - возвращаем, иначе переходим к следующему филднейму
                foreach (Tuple<string,bool> fieldName in fieldNames)
                {
                    FieldInfo field = typeof(Entry).GetField(fieldName.Item1, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (field != null)
                    {
                        if (field.GetValue(x).GetType() == typeof(bool))
                        {
                            bool xx = (bool)field.GetValue(x);
                            bool yy = (bool)field.GetValue(y);
                            if (xx && !yy && fieldName.Item2 || !xx && yy && !fieldName.Item2) return 1;
                            else if (!xx && yy && fieldName.Item2 || xx && !yy && !fieldName.Item2) return -1;
                        }
                        else if (field.GetValue(x).GetType() == typeof(int))
                        {
                            int xx = (int)field.GetValue(x);
                            int yy = (int)field.GetValue(y);
                            if (xx > yy && fieldName.Item2 || yy > xx && !fieldName.Item2) return 1;
                            else if (yy > xx && fieldName.Item2 || xx>yy && !fieldName.Item2) return -1;
                        }
                        else if (field.GetValue(x).GetType() == typeof(float))
                        {
                            float xx = (float)field.GetValue(x);
                            float yy = (float)field.GetValue(y);
                            if (xx > yy && fieldName.Item2 || yy > xx && !fieldName.Item2) return 1;
                            else if (yy > xx && fieldName.Item2 || xx > yy && !fieldName.Item2) return -1;
                        }
                        else if (field.GetValue(x).GetType() == typeof(string))
                        {
                            string xx = (string)field.GetValue(x);
                            string yy = (string)field.GetValue(y);
                            int comp = String.Compare(xx, yy);
                            if (comp > 0 && fieldName.Item2 || comp < 0 && !fieldName.Item2) return 1;
                            else if (comp < 0 && fieldName.Item2 || comp > 0 && !fieldName.Item2) return -1;
                        }
                        else { errorDescription = "Very strange error: wrong typeof() in sorting. You should debug it."; return 0; }
                    }
                    else { errorDescription = "Wrong value name: " + field; return 0; }
                }
                return 0;
            });
        }

        //formatting = formattingToken '=' "'" formattingValue "'"
        private string Format(Tokenizer t, Database db)
        {
            GetToken(t);
            if (token == Token.formattingToken.ToLower())
            {
                GetToken(t);
                if (token == "=")
                {
                    GetToken(t);
                    if (token == "'")
                    {
                        //Создаём выходную переменную
                        string output = "";
                        Tokenizer T = new Tokenizer(t.input.Substring(t.pos));
                        //Для каждой карты будем дописывать одну строку в аутпут
                        //Но если список карт вдруг пуст - проматываем токенайзер. Выполнить код придётся, потому что где-то внутри может быть одинарная кавычка внутри двойных, и получение текста до одинарной кавычки может закончиться ошибкой.
                        bool dbIsEmpty = false;
                        string emptyOutput = output;
                        if (db.cardList.Count == 0) { dbIsEmpty = true; db.cardList.Add(new Entry()); }
                        foreach (Entry card in db.cardList)
                        {
                            //Для каждой карты создаём свой токенайзер
                            T = new Tokenizer(t.input.Substring(t.pos));
                            //formattingValue = ([function] [field] [freeText])*;
                            while (!T.endIsReached && GetNextToken(T) != "'")
                            {
                                if (GetNextToken(T) == "$")
                                {
                                    output += T.GetWhiteSpaces();
                                    GetToken(T);
                                    //Дальше это либо функция, либо поле.
                                    if (GetNextToken(T) == "(")
                                    {
                                        Tuple<string, bool, string, float> result = EmptyFunction(T, card);
                                        if (errorDescription != null) return output;
                                        switch (result.Item1)
                                        {
                                            case "bool":
                                                output += result.Item2.ToString();
                                                break;
                                            case "string":
                                                output += result.Item3;
                                                break;
                                            case "number":
                                                output += result.Item4.ToString();
                                                break;
                                        }
                                    }
                                    else //либо поле, либо непустая функция
                                    {
                                        Tokenizer tempT = new Tokenizer(T.input.Substring(T.pos));
                                        string stringField = StringField(tempT, card);
                                        if (errorDescription != null)
                                        {
                                            tempT.SetPos(0);
                                            errorDescription = null;
                                            float numberField = NumberField(tempT, card);
                                            if (errorDescription != null)
                                            {
                                                tempT.SetPos(0);
                                                errorDescription = null;
                                                bool boolField = BoolField(tempT, card);
                                                if (errorDescription != null)
                                                {
                                                    tempT.SetPos(0);
                                                    errorDescription = null;
                                                    string stringResult = StringFunction(tempT, card);
                                                    if (errorDescription != null)
                                                    {
                                                        tempT.SetPos(0);
                                                        errorDescription = null;
                                                        float numberResult = NumberFunction(tempT, card);
                                                        if (errorDescription != null)
                                                        {
                                                            tempT.SetPos(0);
                                                            errorDescription = null;
                                                            bool boolResult = BoolFunction(tempT, card);
                                                            if (errorDescription != null) { errorDescription = "Parsing failed. That may be because of wrong field name or wrong function return type. Check the synthax."; return output; }
                                                            else output += boolResult;
                                                        }
                                                        else output += numberResult;
                                                    }
                                                    else output += stringResult;
                                                }
                                                else output += boolField;
                                            }
                                            else output += numberField;
                                        }
                                        else output += stringField;

                                        T.SetPos(tempT.pos + T.input.Length - tempT.input.Length);
                                    }
                                }
                                else output += T.GetSymbol();
                            }
                            //после того, как закончили парсить формулу - переводим строку, переходим к следующей карте, для которой заново будем парсить формулу.
                            //Если база была пуста - ничего не добавляем. Мы просто промотали токенайзер.
                            if (!dbIsEmpty) output += "\r\n";
                            else output = emptyOutput;
                        }
                        t.SetPos(T.pos + t.input.Length - T.input.Length);
                        GetToken(t);
                        if (token != "'") { errorDescription = ErrorQuotes("define formatting"); return output; }
                        return output;
                    }
                    else { errorDescription = ErrorQuotes("define formatting"); return ""; }
                }
                else { errorDescription = ErrorExpected("="); return ""; }
            }
            else { errorDescription = ErrorExpected(Token.formattingToken); return ""; }
        }

        
        #region Operators

        //value = boolValue | numberValue | stringValue
        private Tuple<string, bool, string, float> Value(Tokenizer t, Entry card)
        {
            Tokenizer T = new Tokenizer(t.input.Substring(t.pos));
            Tuple<string, bool, string, float> output;
            bool boolResult = BoolValue(T, card);
            if (errorDescription != null)
            {
                T.SetPos(0);
                errorDescription = null;
                float numberResult = NumberValue(T, card);
                if (errorDescription != null)
                {
                    T.SetPos(0);
                    errorDescription = null;
                    string stringResult = StringValue(T, card);
                    if (errorDescription != null) { errorDescription = "Parsing failed. That may be because of wrong field name or synthax."; return new Tuple<string, bool, string, float>("error", false, null, 0); }
                    else output = new Tuple<string, bool, string, float>("string", false, stringResult, 0);
                }
                else output = new Tuple<string, bool, string, float>("number", false, null, numberResult);
            }
            else output = new Tuple<string, bool, string, float>("bool", boolResult, null, 0);
            t.SetPos(T.pos + t.input.Length - T.input.Length);
            return output;
        }

        //emptyFunction = brackets
        //brackets = boolBrackets | numberBrackets | stringBrackets; (* '(' value ')' *)
        //brackets = '(' value ')'
        private Tuple<string, bool, string, float> EmptyFunction(Tokenizer t, Entry card)
        {
            GetToken(t);
            if (token == "(")
            {
                Tuple<string, bool, string, float> output = Value(t, card);
                if (errorDescription != null) return output;
                GetToken(t);
                if (token == ")") return output;
                else { errorDescription = ErrorExpected(")"); return output; }
            }
            else { errorDescription = ErrorExpected("("); return new Tuple<string, bool, string, float>("error", false, null, 0); }
        }

        #region Bool operators

        //boolValue = or
        //or = and (('|') and)*
        private bool BoolValue(Tokenizer t, Entry card)
        {
            bool left = And(t, card);
            if (errorDescription != null) return left;
            while (GetNextToken(t) == "|")
            {
                GetToken(t); //'|'
                bool right = And(t, card);
                left = left || right;
            }
            return left;
        }

        //and = equality (('&') equality)*
        private bool And(Tokenizer t, Entry card)
        {
            bool left = Equality(t, card);
            if (errorDescription != null) return left;
            while (GetNextToken(t) == "&")
            {
                GetToken(t);
                bool right = Equality(t, card);
                if (errorDescription != null) return left && right;
                left = left && right;
            }
            return left;
        }

        //equality = (comparsion (('=' | '!=') comparsion)*) | (numberValue (('=' | '!=') numberValue)+) | (stringValue (('=' | '!=') stringValue)+)
        private bool Equality(Tokenizer t, Entry card)
        {
            //создаём отдельный токенайзер
            Tokenizer T = new Tokenizer(t.input.Substring(t.pos));
            bool boolLeft = Comparsion(T, card);
            bool output = boolLeft;
            //если парсинг как сущность boolValue НЕ выдаёт ошибку - ищем дальше связки ('=' | '!=') comparsion.
            if (errorDescription == null)
            {
                bool firstBypass = true;
                while (GetNextToken(T) == "=" || GetNextToken(T) == "!=")
                {
                    GetToken(T);
                    if (token == "=")
                    {
                        bool boolRight = Comparsion(T, card);
                        if (firstBypass) output = boolLeft == boolRight;
                        else output = output && boolLeft == boolRight;
                        boolLeft = boolRight;
                    }
                    else
                    {
                        bool boolRight = Comparsion(T, card);
                        if (firstBypass) output = boolLeft != boolRight;
                        else output = output && boolLeft != boolRight;
                        boolLeft = boolRight;
                    }
                    if (errorDescription != null) return output;
                    firstBypass = false;
                }
            }
            //если парсинг как сущность comparsion выдаёт ошибку - пытаемся парсить как сущность numberValue
            else
            {
                T.SetPos(0);
                errorDescription = null;
                float numberLeft = NumberValue(T, card);
                //если парсинг как сущность numberValue НЕ выдаёт ошибку - ищем дальше связки ('=' | '!=') numberValue. Должна быть хотя бы одна такая связка.
                if (errorDescription == null)
                {
                    bool firstBypass = true;
                    do
                    {
                        GetToken(T);
                        if (token == "=")
                        {
                            float numberRight = NumberValue(T, card);
                            if (firstBypass) output = numberLeft == numberRight;
                            else output = output && numberLeft == numberRight;
                            numberLeft = numberRight;
                        }
                        else if (token == "!=")
                        {
                            float numberRight = NumberValue(T, card);
                            if (firstBypass) output = numberLeft != numberRight;
                            else output = output && numberLeft != numberRight;
                            numberLeft = numberRight;
                        }
                        else { errorDescription = ErrorExpected("'=' or '!='", false); return output; }
                        if (errorDescription != null) return output;
                        firstBypass = false;
                    }
                    while (GetNextToken(T) == "=" || GetNextToken(T) == "!=");
                }
                //если парсинг как сущность numberValue выдаёт ошибку - пытаемся парсить как сущность stringValue
                else
                {
                    T.SetPos(0);
                    errorDescription = null;
                    string stringLeft = StringValue(T, card);
                    //если парсинг как сущность stringValue НЕ выдаёт ошибку - ищем дальше связки ('=' | '!=') stringValue. Должна быть хотя бы одна такая связка.
                    if (errorDescription == null)
                    {
                        bool firstBypass = true;
                        do
                        {
                            GetToken(T);
                            if (token == "=")
                            {
                                string stringRight = StringValue(T, card);
                                if (firstBypass) output = stringLeft.ToLower() == stringRight.ToLower();
                                else output = output && stringLeft.ToLower() == stringRight.ToLower();
                                stringLeft = stringRight;
                            }
                            else if (token == "!=")
                            {
                                string stringRight = StringValue(T, card);
                                if (firstBypass) output = stringLeft.ToLower() != stringRight.ToLower();
                                else output = output && stringLeft.ToLower() != stringRight.ToLower();
                                stringLeft = stringRight;
                            }
                            else { errorDescription = ErrorExpected("'=' or '!='", false); return output; }
                            if (errorDescription != null) return output;
                            firstBypass = false;
                        }
                        while (GetNextToken(T) == "=" || GetNextToken(T) == "!=");
                    }
                    else { errorDescription = "Parsing failed. Bool value expected. Check the synthax."; return output; }
                }
            }
            t.SetPos(T.pos + t.input.Length - T.input.Length);
            return output;
        }

        //comparsion = boolArg | numberValue (('>' | '<' | '>=' | '<=') numberValue)+
        private bool Comparsion(Tokenizer t, Entry card)
        {
            //создаём отдельный токенайзер
            Tokenizer T = new Tokenizer(t.input.Substring(t.pos));
            bool output = BoolArg(T, card);
            //если парсинг как сущность boolArg выдаёт ошибку - пытаемся парсить как сущность numberValue
            if (errorDescription != null)
            {
                T.SetPos(0);
                errorDescription = null;
                float numberLeft = NumberValue(T, card);
                //если парсинг как сущность boolValue НЕ выдаёт ошибку - ищем дальше связки ('>' | '>=' | '<' | '<=') boolValue . Должна быть хотя бы одна такая связка.
                if (errorDescription == null)
                {
                    GetToken(T);

                    if (token == ">")
                    {
                        float numberRight = NumberValue(T, card);
                        output = numberLeft > numberRight;
                        numberLeft = numberRight;
                    }
                    else if (token == ">=")
                    {
                        float numberRight = NumberValue(T, card);
                        output = numberLeft >= numberRight;
                        numberLeft = numberRight;
                    }
                    else if (token == "<")
                    {
                        float numberRight = NumberValue(T, card);
                        output = numberLeft < numberRight;
                        numberLeft = numberRight;
                    }
                    else if (token == "<=")
                    {
                        float numberRight = NumberValue(T, card);
                        output = numberLeft <= numberRight;
                        numberLeft = numberRight;
                    }
                    else { errorDescription = ErrorExpected("'>', '>=', '<', or '<='", false); return output; }
                    if (errorDescription != null) return output;
                }
                else { errorDescription = "Parsing failed. Bool value expected. Check the synthax."; return output; }
            }
            //если парсинг как сущность boolArg НЕ выдаёт ошибку - возвращаем полученное значение
            t.SetPos(T.pos + t.input.Length - T.input.Length);
            return output;
        }

        //boolArg = ['!'] ('$' (boolFunction | boolField)) | boolBrackets |  bool
        private bool BoolArg(Tokenizer t, Entry card)
        {
            bool reverseOutput = false;
            bool output;
            if (GetNextToken(t) == "!") { reverseOutput = true; GetToken(t); }

            if (GetNextToken(t) == "$")
            {
                GetToken(t);

                //Дальше это либо поле, либо непустая функция.
                Tokenizer T = new Tokenizer(t.input.Substring(t.pos));
                output = BoolField(T, card);
                if (errorDescription != null)
                {
                    T.SetPos(0);
                    errorDescription = null;
                    bool boolResult = BoolFunction(T, card);
                    output = boolResult;
                    if (errorDescription != null) { errorDescription = "Parsing failed. Wrong field or function error.\r\nParsing as bool function ended with an error:\r\n" + errorDescription; return reverseOutput != output; }
                }
                t.SetPos(T.pos + t.input.Length - T.input.Length);
            }
            else if (GetNextToken(t) == "(")
            {
                output = BoolBrackets(t, card);
                if (errorDescription != null) return reverseOutput != output;
            }
            //bool = 'true' | 'false'
            else if (GetNextToken(t).ToLower() == "true") { GetToken(t); output = true; }
            else if (GetNextToken(t).ToLower() == "false") { GetToken(t); output = false; }
            else { GetToken(t); errorDescription = ErrorExpected(); return false; }

            return reverseOutput != output;
        }

        //boolFunction = contains
        private bool BoolFunction(Tokenizer t, Entry card)
        {
            bool output;
            if (GetNextToken(t).ToLower() == "contains") output = Contains(t, card);
            else { GetToken(t); errorDescription = "Unknown function: " + token; output = false; }
            return output;
        }
        
        //contains = 'contains' '(' stringArg ',' stringArg ')'
        private bool Contains(Tokenizer t, Entry card)
        {
            GetToken(t);
            if (token.ToLower() == "contains")
            {
                GetToken(t);
                if (token == "(")
                {
                    string firstArg = StringValue(t, card);
                    if (errorDescription != null) return false;
                    GetToken(t);
                    if (token == ",")
                    {
                        bool output;
                        string secondArg = StringValue(t, card);
                        output = firstArg.ToLower().Contains(secondArg.ToLower());
                        if (errorDescription != null) return output;

                        GetToken(t);
                        if (token == ")") return output;
                        else { errorDescription = ErrorExpected(")"); return output; }
                    }
                    else { errorDescription = "'contains' function has two inputs: 'first' contains 'second'."; return false; }
                }
                else { errorDescription = ErrorExpected("("); return false; }
            }
            else { errorDescription = ErrorExpected("contains"); return false; }
        }

        //boolBrackets = '(' boolValue ')'
        private bool BoolBrackets(Tokenizer t, Entry card)
        {
            GetToken(t);
            if (token == "(")
            {
                bool output = BoolValue(t, card);
                if (errorDescription != null) return output;
                GetToken(t);
                if (token != ")") errorDescription = ErrorExpected(")");
                return output;
            }
            else { errorDescription = ErrorExpected("("); return false; }
        }

        //boolField = ?boolField?
        private bool BoolField(Tokenizer t, Entry card)
        {
            bool output;
            GetToken(t);
            switch (token.ToLower())
            {
                case "foil":
                    output = card.foil; break;
                default:
                    errorDescription = "Wrong value name: " + token; output = false; break;
            }
            return output;
        }

        #endregion

        #region Number operators

        //numberValue = sum;
        //sum = composition (('+' | '-') composition)*;
        private float NumberValue(Tokenizer t, Entry card)
        {
            float left = Composition(t, card);
            if (errorDescription != null) return left;
            while (GetNextToken(t) == "+" || GetNextToken(t) == "-")
            {
                GetToken(t);
                if (token == "+")
                {
                    float right = Composition(t, card);
                    left = left + right;
                }
                else
                {
                    float right = Composition(t, card);
                    left = left - right;
                }
            }
            return left;
        }

        //composition = numberArg (('*' | '/') numberArg)*
        private float Composition(Tokenizer t, Entry card)
        {
            float left = NumberArg(t, card);
            if (errorDescription != null) return left;
            while (GetNextToken(t) == "*" || GetNextToken(t) == "/")
            {
                GetToken(t);
                if (token == "*")
                {
                    float right = NumberArg(t, card);
                    left = left * right;
                }
                else
                {
                    float right = NumberArg(t, card);
                    left = left / right;
                }
            }
            return left;
        }

        //numberArg = ['-'] ('$' (numberFunction | numberField)) | numberBrackets |  positiveNumber
        private float NumberArg(Tokenizer t, Entry card)
        {
            bool reverseOutput = false;
            float output;
            if (GetNextToken(t) == "-") { reverseOutput = true; GetToken(t); }

            if (GetNextToken(t) == "$")
            {
                GetToken(t);

                //Дальше это либо поле, либо непустая функция.
                Tokenizer T = new Tokenizer(t.input.Substring(t.pos));
                output = NumberField(T, card);
                if (errorDescription != null)
                {
                    T.SetPos(0);
                    errorDescription = null;
                    float numberResult = NumberFunction(T, card);
                    output = numberResult;
                    if (errorDescription != null) { errorDescription = "Parsing failed. Wrong field or function error.\r\nParsing as number function ended with an error:\r\n" + errorDescription; return reverseOutput ? -output : output; }
                }
                t.SetPos(T.pos + t.input.Length - T.input.Length);
            }
            else if (GetNextToken(t) == "(")
            {
                output = NumberBrackets(t, card);
                if (errorDescription != null) return reverseOutput ? -output : output;
            }
            //positiveNumber = positiveInteger ['.' positiveInteger];
            //positiveInteger = digit+;
            //digit = ?digit?;
            else if (float.TryParse(GetNextToken(t).Replace('.', ','), out output)) GetToken(t);
            else { errorDescription = ErrorExpected(); return reverseOutput ? -output : output; }
            
            return reverseOutput ? -output : output;
        }

        //numberFunction = numberIf
        private float NumberFunction(Tokenizer t, Entry card)
        {
            float output;
            if (GetNextToken(t).ToLower() == "if") output = NumberIf(t, card);
            else { GetToken(t); errorDescription = "Unknown function: " + token; output = 0; }
            return output;
        }

        //numberIf = 'if' '(' boolValue ',' numberValue [',' numberValue] ')'
        private float NumberIf(Tokenizer t, Entry card)
        {
            GetToken(t);
            if (token.ToLower() == "if")
            {
                GetToken(t);
                if (token == "(")
                {
                    bool condition = BoolValue(t, card);
                    if (errorDescription != null) return 0;
                    GetToken(t);
                    if (token == ",")
                    {
                        float output;
                        float firstArg = NumberValue(t, card);
                        if (errorDescription != null) return 0;
                        float secondArg;
                        if (GetNextToken(t) == ",")
                        {
                            GetToken(t);
                            secondArg = NumberValue(t, card);
                        }
                        else secondArg = 0;
                        output = condition ? firstArg : secondArg;
                        if (errorDescription != null) return output;
                        GetToken(t);
                        if (token != ")") errorDescription = ErrorExpected(")");
                        return output;
                    }
                    else { errorDescription = "'if' function has two or three inputs: if 'first' is true, return 'second', else - if 'third' is defined, return 'third', else return 0."; return 0; }
                }
                else { errorDescription = ErrorExpected("("); return 0; }
            }
            else { errorDescription = ErrorExpected("if"); return 0; }
        }

        //numberBrackets = '(' numberValue ')'
        private float NumberBrackets(Tokenizer t, Entry card)
        {
            GetToken(t);
            if (token == "(")
            {
                float output = NumberValue(t, card);
                if (errorDescription != null) return output;
                GetToken(t);
                if (token != ")") errorDescription = ErrorExpected(")");
                return output;
            }
            else { errorDescription = ErrorExpected("("); return 0; }
        }

        //numberField = ?numberField?
        private float NumberField(Tokenizer t, Entry card)
        {
            float output;
            GetToken(t);
            switch (token.ToLower())
            {
                case "buyprice":
                    output = card.buyPrice; break;
                case "buyqty":
                    output = card.buyQty; break;
                case "pricef":
                    output = card.priceF; break;
                case "pricer":
                    output = card.priceR; break;
                case "proxies":
                    output = card.proxies; break;
                case "qtyf":
                    output = card.qtyF; break;
                case "qtyr":
                    output = card.qtyR; break;
                case "rating":
                    output = card.rating; break;
                case "sellprice":
                    output = card.sellPrice; break;
                case "sellqty":
                    output = card.sellQty; break;
                case "used":
                    output = card.used; break;
                case "qty":
                    output = card.qty; break;
                case "dollarrate":
                    output = card.dollarRate; break;
                case "discount":
                    output = card.discount; break;
                case "originalprice":
                    output = card.originalPrice; break;
                case "price":
                    output = card.price; break;
                case "priority":
                    output = card.priority; break;
                case "groupid":
                    output = card.groupID; break;
                default:
                    errorDescription = "Wrong value name: " + token; output = 0; break;
            }
            return output;
        }

        #endregion

        #region String operators

        //stringValue = stringSum
        //stringSum = stringArg ('+' stringArg)*
        private string StringValue(Tokenizer t, Entry card)
        {
            string left = StringArg(t, card);
            if (errorDescription != null) return left;
            while (GetNextToken(t) == "+")
            {
                GetToken(t);
                string right = StringArg(t, card);
                left = left + right;
            }
            return left;
        }

        //stringArg = ('$' (stringFunction | stringField)) | stringBrackets | string
        private string StringArg(Tokenizer t, Entry card)
        {
            string output;
            
            if (GetNextToken(t) == "$")
            {
                GetToken(t);

                //Дальше это либо поле, либо непустая функция.
                Tokenizer T = new Tokenizer(t.input.Substring(t.pos));
                output = StringField(T, card);
                if (errorDescription != null)
                {
                    T.SetPos(0);
                    errorDescription = null;
                    string stringResult = StringFunction(T, card);
                    output = stringResult;
                    if (errorDescription != null) { errorDescription = "Parsing failed. Wrong field or function error.\r\nParsing as string function ended with an error:\r\n" + errorDescription; return output; }
                }
                t.SetPos(T.pos + t.input.Length - T.input.Length);
            }
            else if (GetNextToken(t) == "(")
            {
                output = StringBrackets(t, card);
                if (errorDescription != null) return output;
            }
            else if (GetNextToken(t) == "\"")
            {
                GetToken(t);
                output = t.GetUntil('"');
                GetToken(t);
                if (token != "\"") { errorDescription = ErrorExpected("\""); }
            }
            else { errorDescription = ErrorExpected(); return ""; } 

            return output;
        }

        //stringFunction = stringIf | toString
        private string StringFunction(Tokenizer t, Entry card)
        {
            string output;
            if (GetNextToken(t).ToLower() == "if") output = StringIf(t, card);
            else if (GetNextToken(t).ToLower() == "tostring") output = ToString(t, card);
            else { GetToken(t); errorDescription = "Unknown function: " + token; output = ""; }
            return output;
        }

        //stringIf = 'if' '(' boolValue ',' stringValue [',' stringValue] ')'
        private string StringIf(Tokenizer t, Entry card)
        {
            GetToken(t);
            if (token.ToLower() == "if")
            {
                GetToken(t);
                if (token == "(")
                {
                    bool condition = BoolValue(t, card);
                    if (errorDescription != null) return "";
                    GetToken(t);
                    if (token == ",")
                    {
                        string output;
                        string firstArg = StringValue(t, card);
                        if (errorDescription != null) return "";
                        string secondArg;
                        if (GetNextToken(t) == ",")
                        {
                            GetToken(t);
                            secondArg = StringValue(t, card);
                        }
                        else secondArg = "";
                        output = condition ? firstArg : secondArg;
                        if (errorDescription != null) return output;
                        GetToken(t);
                        if (token != ")") errorDescription = ErrorExpected(")");
                        return output;
                    }
                    else { errorDescription = "'if' function has two or three inputs: if 'first' is true, return 'second', else - if 'third' is defined, return 'third', else return 0."; return ""; }
                }
                else { errorDescription = ErrorExpected("("); return ""; }
            }
            else { errorDescription = ErrorExpected("if"); return ""; }
        }

        //toString = 'toString' '(' value ')'
        private string ToString(Tokenizer t, Entry card)
        {
            GetToken(t);
            if (token.ToLower() == "tostring")
            {
                GetToken(t);
                if (token == "(")
                {
                    string output = "";
                    Tuple<string, bool, string, float> result = Value(t, card);
                    switch (result.Item1)
                    {
                        case "bool":
                            output = result.Item2.ToString();
                            break;
                        case "string":
                            output = result.Item3;
                            break;
                        case "number":
                            output = result.Item4.ToString();
                            break;
                    }

                    GetToken(t);
                    if (token != ")") errorDescription = ErrorExpected(")");
                    return output;
                }
                else { errorDescription = ErrorExpected("("); return ""; }
            }
            else { errorDescription = ErrorExpected("toString"); return ""; }
            
        }

        //stringBrackets = '(' stringValue ')'
        private string StringBrackets(Tokenizer t, Entry card)
        {
            GetToken(t);
            if (token == "(")
            {
                string output = StringValue(t, card);
                if (errorDescription != null) return output;
                GetToken(t);
                if (token != ")") errorDescription = ErrorExpected(")");
                return output;
            }
            else { errorDescription = ErrorExpected("("); return ""; }
        }

        //stringField = ?stringField?
        private string StringField(Tokenizer t, Entry card)
        {
            string output;
            GetToken(t);
            switch (token.ToLower())
            {
                case "artist":
                    output = card.artist; break;
                case "border":
                    output =  card.border; break;
                case "color":
                    output =  card.color; break;
                case "copyright":
                    output =  card.copyright; break;
                case "cost":
                    output =  card.cost; break;
                case "gradef":
                    output =  card.gradeF; break;
                case "grader":
                    output =  card.gradeR; break;
                case "language":
                    output =  card.language; break;
                case "legality":
                    output =  card.legality; break;
                case "name":
                    output =  card.name; break;
                case "nameoracle":
                    output =  card.nameOracle; break;
                case "notes":
                    output =  card.notes; break;
                case "number":
                    output =  card.number; break;
                case "pt":
                    output =  card.pt; break;
                case "rarity":
                    output =  card.rarity; break;
                case "set":
                    output =  card.set; break;
                case "text":
                    output =  card.text; break;
                case "textoracle":
                    output =  card.textOracle; break;
                case "type":
                    output =  card.type; break;
                case "typeoracle":
                    output =  card.typeOracle; break;
                case "version":
                    output =  card.version; break;
                case "comment":
                    output =  card.comment; break;
                case "grade":
                    output =  card.grade; break;
                default: errorDescription = "Wrong value name: " + token; output = ""; break;
            }
            return output;
        }

        #endregion

        #endregion

        //Разукрашиваем токены в кейворде
        //public void Paint(System.Windows.Forms.RichTextBox box)
        //{
        //    int initPos = box.SelectionStart;
        //    int initLength = box.SelectionLength;
        //    box.SelectAll();
        //    box.SelectionColor = Color.Black;
        //    box.ForeColor = Color.Black;
        //    Tokenizer t = new Tokenizer(input);

        //    while (!t.endIsReached)
        //    {
        //        bool got = false;
        //        GetToken(t);
        //        foreach (KeyValuePair<string, string> pair in keyWords)
        //        {
        //            if (token.ToLower() == pair.Value.ToLower())
        //            {
        //                got = true;
        //                if (Char.IsWhiteSpace(t.input[t.pos - 1])) { t.SetPos(t.pos - 1); }
        //                box.Select(t.pos - token.Length, token.Length);
        //                box.SelectionColor = Color.Blue;
        //            }
        //        }
        //        if (got) continue;
        //        if (token == "\"")
        //        {
        //            got = true;
        //            token = t.GetUntil('"');
        //            box.Select(t.pos - token.Length - 1, token.Length + 2);
        //            box.SelectionColor = Color.FromArgb(163, 21, 21);
        //            GetToken(t);
        //        }
        //        if (got) continue;
        //        if (token == "$")
        //        {
        //            got = true;
        //            GetToken(t);
        //            if (token == "(")
        //            {
        //                box.Select(t.pos - 2, 1);
        //                box.SelectionColor = Color.FromArgb(43, 145, 175);
        //            }
        //            else if (token.ToLower() == "if")
        //            {
        //                GetToken(t);
        //                if (token == "(")
        //                {
        //                    box.Select(t.pos - 4, 3);
        //                    box.SelectionColor = Color.FromArgb(43, 145, 175);
        //                }
        //            }
        //            else if (token.ToLower() == "contains")
        //            {
        //                box.Select(t.pos - 9, 9);
        //                box.SelectionColor = Color.FromArgb(43, 145, 175);
        //            }
        //            else if (token.ToLower() == "tostring")
        //            {
        //                box.Select(t.pos - 9, 9);
        //                box.SelectionColor = Color.FromArgb(43, 145, 175);
        //            }
        //            else if (token != "|" && token != "&" && token != "=" && token != "+" && token != "-" && token != "*" && token != "/" && token != ")" && token != "," && token != "$" && token != "!=" && token != ">" && token != "<" && token != ">=" && token != "<=" && token != "'" && token != "\"" && token != "")
        //            {
        //                if (Char.IsWhiteSpace(t.input[t.pos - 1])) { t.SetPos(t.pos - 1); }
        //                box.Select(t.pos - token.Length - 1, token.Length + 1);
        //                box.SelectionColor = Color.OrangeRed;
        //            }
        //        }
        //    }
        //    box.SelectionStart = initPos;
        //    box.SelectionLength = initLength;
        //    box.SelectionColor = Color.Black;
        //}

    }
}
