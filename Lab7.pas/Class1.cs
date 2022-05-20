using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Lab7.pas
{

    class Class1
    {
        struct Lexemme
        {
            public int str_N;
            public int lex_N;
            public string name;
        }
        string[] reg1 = { "AL", "CL", "DL", "BL", "AH", "CH", "BH", "DH" };
        string[] reg2 = { "AX", "CX", "DX", "BX", "SP", "BP", "SI", "DI" };
        string[] sreg = { "ES", "SS", "DS" };
        string cs = "CS", db = "DB", dw = "DW", mov = "MOV", push = "PUSH", imul = "IMUL";
        int curr_lex = 0;
        int str_index = 1;
        string strg="";
        Lexemme[] lex_table = new Lexemme[100];

        public void exec()
        {
            foreach (string line in File.ReadLines("test.txt"))
            {
                string_analize(line.ToUpper(), str_index);
                str_index++;
                strg = line.ToUpper();
            }
            string_analize(strg, str_index);
            using (StreamWriter fstream = new StreamWriter("res.txt", false))
            {
                for (int i = 0; lex_table[i].str_N != 0; i++) 
                {
                    fstream.WriteLine(lex_table[i].str_N + " " + lex_table[i].lex_N + " " + lex_table[i].name);
                }
            }
        }
        private bool is_reg1(string s)
        {
            for (int i = 0; i < 8; i++)
            {
                if (s == reg1[i])
                {
                    lex_table[curr_lex].name = reg1[i];
                    return true;
                }
            }
            return false;
        }
        private bool is_reg2(string s)
        {
            for (int i = 0; i < 8; i++)
            {
                if (s == reg2[i])
                {
                    lex_table[curr_lex].name = reg2[i];
                    return true;
                }
            }
            return false;
        }
        private bool is_sreg(string s)
        {
            for (int i = 0; i < 3; i++)
            {
                if (s == sreg[i])
                {
                    lex_table[curr_lex].name = sreg[i];
                    return true;
                }
            }
            return false;
        }
        private void word_analize(string s, int i)
        {
            int No;
            lex_table[curr_lex].str_N = i;
            if (s == ",") { No = 1; }
            else if (s == db) { No = 2; }
            else if (s == dw) { No = 3; }
            else if (s == mov) { No = 4; }
            else if (s == push) { No = 5; }
            else if (s == imul) { No = 6; }
            else if (is_reg1(s)) { No = 7; }
            else if (is_reg2(s)) { No = 8; }
            else if (is_sreg(s)) { No = 9; }
            else if (s == cs) { No = 10; }
            else if (s[0] >= '0' && s[0] <= '9') { No = 11; }
            else No = 12;
            lex_table[curr_lex].lex_N = No;
            if (No == 12) 
            {
                if (s.Length > 7) lex_table[curr_lex].name = s.Substring(0, 7);
                else lex_table[curr_lex].name = s;
            }
            else if (No == 11) { lex_table[curr_lex].name = s; }
            curr_lex++;
        }
        private void string_analize(string s, int i)
        {
            string[] words = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int j = 0; j < words.Length; j++)
            {
                if (words[j].Contains(','))
                {
                    if (words[j].Length == 1)
                    {
                        word_analize(",", i);
                    }
                    else
                    {
                        int indexOfComma = words[j].IndexOf(',');
                        word_analize(words[j].Substring(0, indexOfComma), i);
                        word_analize(",", i);
                        word_analize(words[j].Substring(indexOfComma + 1), i);
                    }
                }
                else
                {
                    word_analize(words[j], i);
                }

            }
        }
        private int number(int i)
        {
            int n, code, integer;
            int number = 0;
            n = lex_table[i].name.Length;
            if (lex_table[i].name[n - 1] == 'H')
            {
                for (code = 0; code < n - 1; code++)
                {
                    if (Regex.IsMatch(lex_table[i].name[code].ToString(), "[0-9A-Z]")) return number;
                }
                if (n > 6) return number;
                if ((lex_table[i].name[0] >= 'A' || n >= 6) && lex_table[i].name[0] == '0') return number;
                if (lex_table[i].name[0] == '0')
                {
                    if (n > 4) number = 2;
                    else number = 1;
                }
                if (lex_table[i].name[0] != '0')
                {
                    if (n > 3) number = 2;
                    else number = 1;
                }
            }
            else
            {
                bool success = Int32.TryParse(lex_table[i].name, out integer);
                if (!success) return number;
                if (integer < 256) number = 1;
                else if (integer < 65536) number = 2;
                else number = 0;
            }
            return number;
        }
        private bool check_already_db(int n)
        {
            for (int i = 0; i < n - 1; i++)
            {
                if (lex_table[i].name == lex_table[n].name && lex_table[i + 1].lex_N == 2)
                {
                    return true;
                }
            }
            return false;
        }
        private bool check_already_dw(int n)
        {
            for (int i = 0; i < n - 1; i++)
            {
                if (lex_table[i].name == lex_table[n].name && lex_table[i + 1].lex_N == 3)
                {
                    return true;
                }
            }
            return false;
        }
        private bool _db(int i)
        {
            if (lex_table[i].lex_N == 12)
            {
                if (lex_table[i + 1].lex_N == 2)
                {
                    if (lex_table[i + 2].lex_N == 11 && number(i + 2) == 1)
                    {
                        if (!check_already_db(i) && !check_already_dw(i))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private bool _dw(int i)
        {
            if (lex_table[i].lex_N == 12)
            {
                if (lex_table[i + 1].lex_N == 3)
                {
                    if (lex_table[i + 2].lex_N == 11 && number(i + 2) != 0)
                    {
                        if (!check_already_db(i) && !check_already_dw(i))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private bool _mov(int i)
        {
            if (lex_table[i + 2].lex_N != 1 || lex_table[i].lex_N != 4) return false;
            if (lex_table[i + 1].lex_N == 8)
            {
                if (lex_table[i + 3].lex_N == 9) return true;
                if (lex_table[i + 3].lex_N == 12 && check_already_dw(i + 3)) return true;
                if (lex_table[i + 3].lex_N == 8) return true;
                if (lex_table[i + 3].lex_N == 11 && number(i + 3) > 0) return true;
            }
            if (lex_table[i + 1].lex_N == 7)
            {
                if (lex_table[i + 3].lex_N == 7) return true;
                if (lex_table[i + 3].lex_N == 12 && check_already_db(i + 3)) return true;
                if (lex_table[i + 3].lex_N == 8) return true;
                if (lex_table[i + 3].lex_N == 11 && number(i + 3) == 1) return true;
            }
            if (lex_table[i + 1].lex_N == 9)
            {
                if (lex_table[i + 3].lex_N == 12 && check_already_dw(i + 3)) return true;
                if (lex_table[i + 3].lex_N == 8) return true;
            }
            if (lex_table[i + 1].lex_N == 12 && check_already_dw(i + 1))
            {
                if (lex_table[i + 3].lex_N == 9) return true;
                if (lex_table[i + 3].lex_N >= 8 && lex_table[i + 3].lex_N <= 10) return true;
                if (lex_table[i + 3].lex_N == 11 && number(i + 3) > 0) return true;
            }
            if (lex_table[i + 1].lex_N == 12 && check_already_db(i + 1))
            {
                if (lex_table[i + 3].lex_N == 7) return true;
                if (lex_table[i + 3].lex_N == 11 && number(i + 3) == 1) return true;
            }
            return false;
        }
        private bool _imul(int i)
        {
            if (lex_table[i].lex_N == 6)
            {
                if (lex_table[i + 1].lex_N == 7 || lex_table[i + 1].lex_N == 8) return true;
                if (lex_table[i + 1].lex_N == 12)
                {
                    if (check_already_dw(i + 1) || check_already_db(i + 1)) return true;
                }
            }
            return false;
        }
        private bool _push(int i)
        {
            if (lex_table[i].lex_N == 5)
            {
                if (lex_table[i + 1].lex_N >= 8 && lex_table[i + 1].lex_N < 10) return true;
                else if (lex_table[i + 1].lex_N == 12 && check_already_dw(i + 1)) return true;
            }
            return false;
        }
        private int synth_analize(int err)
        {
            int i = 0, cur_str_N = 0;
            while (lex_table[i].str_N != 0)
            {
                if (!_db(i))
                {
                    if (!_dw(i))
                    {
                        if (!_mov(i))
                        {
                            if (!_push(i))
                            {
                                if (!_imul(i))
                                {
                                    err = cur_str_N;
                                    return err;
                                }
                            }
                        }
                    }
                }
                while (lex_table[i].str_N == cur_str_N) { i++; }
                cur_str_N++;
            }
            return 0;
        }
    }
}
