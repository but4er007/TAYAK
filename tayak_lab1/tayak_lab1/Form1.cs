using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace tayak_lab1
{
    public partial class Form1 : Form
    {
        enum symbolType { UN_OPERAND, STRONG_OPERAND, WEAK_OPERAND, LETTER_OPERAND, BRACKET, DIGIT, COMMA, LETTER, DOT, INVALID }

        //*******************************красивый интерфейс*************************************//
        public Form1()
        {
            InitializeComponent();
        }

        private void textBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (textBox.Text == "Input your expression") textBox.Text = "";
        }

        private void textBox_Leave(object sender, EventArgs e)
        {
            if (textBox.Text == "") textBox.Text = "Input your expression";
        }

        private void button_Click(object sender, EventArgs e)
        {
            labelErrors.Text = "";
            label.Text = "";
            Analizing();
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Return) button_Click(sender, e);
        }

        //**************************************Логика*******************************//
        private void Analizing() 
        {
            if (ThereAreErrors(textBox.Text))
            {
                return;
            }
            Stack opz = new Stack();
            if (!ComputeOPZ(opz)) return; 
            labelErrors.Text += "\n";
            Compute(opz);
        }

        //***************************Обратная польская запись************************//
        private bool ComputeOPZ(Stack opz) 
        {
            Stack stack = new Stack();
            Stack output = new Stack();
            String formula = textBox.Text;
            for (int i = 0; i < formula.Length; i++)
            {
                //обработка скобок
                if (formula[i] == '(')
                {
                    stack.Push(formula[i]);
                }
                if (formula[i] == ')')
                {
                    while (String.Compare(Convert.ToString(stack.Peek()), "(") != 0)
                    {
                        output.Push(stack.Pop());
                    }
                    stack.Pop();
                }
                //обработка букв
                if (Char.IsLetter(formula[i]))
                {
                    String oper = "";
                    oper += formula[i];
                    while (Char.IsLetter(formula[i + 1]))
                    {
                        oper += formula[++i];
                    }
                    if (TermType(oper) != symbolType.LETTER_OPERAND)
                    {
                        labelErrors.Text = "Встретился неизвестный оператор "+oper+"\n";
                        return false;
                    }
                    int brackets = 1;
                    int comma=0;
                    int k = i + 2;
                    while (brackets != 0) 
                    {
                        if (formula[k] == '(') brackets++;
                        if (formula[k] == ')') brackets--;
                        if (formula[k] == ',')
                            if (brackets == 1 && comma == 0) comma++;
                            else 
                            {
                                labelErrors.Text = "Неверный синтаксис использования оператора "+oper+"\n";
                                return false;
                            }
                        k++;
                    }
                    if (comma == 0 || brackets > 0) 
                    {
                        labelErrors.Text = "Неверный синтаксис использования оператора " + oper + "\n";
                        return false;
                    }
                    stack.Push(oper);
                }
                //обработка цифр
                if (Char.IsDigit(formula[i]))
                {
                    String num = "";
                    num += formula[i];
                    if ((i + 1) < formula.Length)
                        while (Char.IsDigit(formula[i + 1]) || formula[i + 1] == '.')
                        {
                            num += formula[++i];
                            if ((i + 1) == formula.Length) break;
                        }
                    output.Push(num);
                }
                //обработка операций
                if (IsOperand(Convert.ToString(formula[i])))
                {
                    switch (TermType(formula[i])) 
                    {
                        case symbolType.UN_OPERAND:
                            if (stack.Count != 0)
                                while (TermType(Convert.ToString(stack.Peek())) == symbolType.LETTER_OPERAND)
                                {
                                    output.Push(stack.Pop());
                                    if (stack.Count == 0) break;
                                }
                                stack.Push(formula[i]);
                                break;
                        case symbolType.WEAK_OPERAND:
                            {
                                if (stack.Count != 0)
                                    while (TermType(Convert.ToString(stack.Peek())) == symbolType.WEAK_OPERAND ||
                                   TermType(Convert.ToString(stack.Peek())) == symbolType.LETTER_OPERAND ||
                                   TermType(Convert.ToString(stack.Peek())) == symbolType.UN_OPERAND)
                                    {
                                        output.Push(stack.Pop());
                                        if (stack.Count == 0) break;
                                    }
                                stack.Push(formula[i]);
                                break;
                            }
                        case symbolType.STRONG_OPERAND:
                            {
                                if (i - 1 == -1) 
                                {
                                    String num = "";
                                    num += formula[i];
                                    if ((i + 1) < formula.Length)
                                        while (Char.IsDigit(formula[i + 1]) || formula[i + 1] == '.')
                                        {
                                            num += formula[++i];
                                            if ((i + 1) == formula.Length) break;
                                        }
                                    output.Push(num);
                                    break;
                                }
                                if(formula[i]=='-' && (formula[i-1]=='(' || formula[i-1]==','))
                                {
                                    String num = "";
                                    num += formula[i];
                                    if ((i + 1) < formula.Length)
                                        while (Char.IsDigit(formula[i + 1]) || formula[i + 1] == '.')
                                        {
                                            num += formula[++i];
                                            if ((i + 1) == formula.Length) break;
                                        }
                                    output.Push(num);
                                    break;
                                }
                                if (stack.Count != 0)
                                    while (TermType(Convert.ToString(stack.Peek())) == symbolType.STRONG_OPERAND ||
                                   TermType(Convert.ToString(stack.Peek())) == symbolType.LETTER_OPERAND ||
                                   TermType(Convert.ToString(stack.Peek())) == symbolType.WEAK_OPERAND ||
                                   TermType(Convert.ToString(stack.Peek())) == symbolType.UN_OPERAND)
                                    {
                                        output.Push(stack.Pop());
                                        if (stack.Count == 0) break;
                                    }
                                stack.Push(formula[i]);
                                break;
                            }
                    }
                }
                //обработка запятой
                if (formula[i] == ',')
                {
                    while (String.Compare(Convert.ToString(stack.Peek()), "(") != 0)
                        output.Push(stack.Pop());
                }
            }
            for (int i = stack.Count; i > 0; i--)
                output.Push(stack.Pop());
            for (int i = output.Count; i > 0; i--)
                opz.Push(output.Pop());
            return true;
        }

        private void Compute(Stack opz) 
        {
            while (opz.Count != 1)
            {
                Stack temp = new Stack();
                while (!IsOperand(Convert.ToString(opz.Peek())))
                    temp.Push(opz.Pop());
                try
                {
                    String oper = Convert.ToString(opz.Pop());
                    double b = Convert.ToDouble(temp.Pop());
                    double a = Convert.ToDouble(temp.Pop());
                    opz.Push(Operation(a, b, oper));
                }
                catch
                {
                    labelErrors.Text = "Ошибка вычисления ответа";
                    return;
                }
                for (int i = temp.Count; i > 0; i--)
                {
                    opz.Push(temp.Pop());
                }
            }
            label.Text = Convert.ToString(opz.Pop());
        }

        private String Operation(double a, double b, String oper) 
        {
            double c;
            switch (oper)
            {
                case "+": 
                    c=a + b;
                    break;
                case "-": 
                    c=a - b;
                    break;
                case "*": 
                    c=a * b;
                    break;
                case "/": 
                    c=a / b;
                    break;
                case "log": 
                    c=Math.Log(a, b);
                    break;
                case "^":
                    c = Math.Pow(a, b);
                    break;
                default: c=0;
                    break;
            }
            return Convert.ToString(c);
        }

        private bool ThereAreErrors(String formula) 
        {
            if (!IsValidSymbols(formula)) 
            {
                labelErrors.Text += "Встретились незнакомые символы\n";
                return true;
            }
            if (!IsValidFormula(formula)) 
            {
                labelErrors.Text += "Неверный формат ввода\n";
                return true;
            }
            CorrectMistakes(formula);
            return false;
        }

        //****************Исправление ошибок*******************//
        private void CorrectMistakes(String formula) 
        {
            if (formula[0] == '-' && formula[1] == '(') formula = formula.Insert(1, "1");
            formula=formula.Replace(")(",")*(");
            for (int i = 0; i < formula.Length; i++) 
            {
                if (formula[i] == ')' && i + 1 != formula.Length) 
                { 
                    if(Char.IsDigit(formula[i+1]) || Char.IsLetter(formula[i+1]))
                        formula=formula.Insert(i+1,"*");
                }                
                if (formula[i] == '(' && i - 1 != -1) 
                { 
                    if(Char.IsDigit(formula[i-1]))
                        formula=formula.Insert(i,"*");
                }
            }
            textBox.Text = formula;
        }
        
        //****************Проверка на ошибки*******************//
        private bool IsValidFormula(String formula) 
        {
            int brackets = 0;
            for (int i = 0; i < formula.Length; i++) 
            {
                switch (formula[i]) 
                { 
                    case '(':
                        if (i + 1 != formula.Length)
                        {
                            if (formula[i + 1] == ')') return false;
                        }
                        else return false;
                        brackets++;
                        break;
                    case ')':
                        if (i - 1 != -1)
                        {
                            if (formula[i - 1] == '(') return false;
                        }
                        else return false;
                        brackets--;
                        break;
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '^':
                        if (i - 1 == -1 && formula[i]!='-')
                            return false;
                        if (i - 1 != -1)
                            if (!(Char.IsDigit(formula[i - 1]) || formula[i - 1] == ')' || 
                              (formula[i] == '-' && (formula[i - 1] == '(' || formula[i - 1] == ',') )))
                                return false;
                        if (i + 1 == formula.Length)
                            return false;
                        if (!(Char.IsDigit(formula[i + 1]) || Char.IsLetter(formula[i + 1]) || 
                          formula[i + 1] == '(' ))
                            return false;
                        break;
                    case '.':
                        if ((i - 1) == -1 || (i + 1) == formula.Length)
                            return false;
                        if (!(Char.IsDigit(formula[i - 1]) && Char.IsDigit(formula[i + 1])))
                            return false;
                        break;
                    case ',':
                        if ((i - 1) == -1 || (i + 1) == formula.Length)
                            return false;
                        if (!((Char.IsDigit(formula[i - 1]) || formula[i - 1] == ')' ) && 
                          (Char.IsDigit(formula[i + 1]) || formula[i + 1] == '(' || formula[i + 1] == '-' || Char.IsLetter(formula[i + 1]))))
                            return false;
                        break;
                    default:
                        if (Char.IsDigit(formula[i]))
                            if (i - 1 != -1)
                                if (Char.IsLetter(formula[i - 1]))
                                    return false;
                        if (Char.IsLetter(formula[i]))
                        {
                            if (i + 1 == formula.Length)
                                return false;
                            if (!(Char.IsLetter(formula[i + 1]) || formula[i + 1] == '('))
                                return false;
                        }
                        break;
                }
                if (brackets < 0) return false;
            }
            if (brackets != 0) return false;
            return true;
        }

        private bool IsValidSymbols(String formula) 
        {
            for (int i = 0; i < formula.Length; i++)
            {
                if (TermType(formula[i]) == symbolType.INVALID) return false;
            }
            return true;
        }

        private symbolType TermType(Char term) 
        {
            switch (term) 
            { 
                case ')':
                case '(':
                    return symbolType.BRACKET;
               case '+':
                case '-':
                    return symbolType.STRONG_OPERAND;
                case '*':
                case '/':
                    return symbolType.WEAK_OPERAND;
                case '^':
                    return symbolType.UN_OPERAND; 
                case '.':
                    return symbolType.DOT;
                case ',':
                    return symbolType.COMMA;
                default:
                    if (Char.IsDigit(term))
                        return symbolType.DIGIT;
                    if (Char.IsLetter(term))
                        return symbolType.LETTER;
                    return symbolType.INVALID;
            }
        }
        
        private symbolType TermType(String term)
        {
            switch (term)
            {
                case ")":
                case "(":
                    return symbolType.BRACKET;
                case "+":
                case "-":
                    return symbolType.STRONG_OPERAND;
                case "*":
                case "/":
                    return symbolType.WEAK_OPERAND;
                case "^":
                    return symbolType.UN_OPERAND; 
                case "log":
                    return symbolType.LETTER_OPERAND;
                case ".":
                    return symbolType.DOT;
                case ",":
                    return symbolType.COMMA;
                default:
                    Regex rxNums = new Regex(@"^\d+$"); // любые цифры
                    if (rxNums.IsMatch(term))
                        return symbolType.DIGIT;
                    return symbolType.INVALID;
            }
        }
        
        private bool IsOperand(String term)
        {
            switch (term)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                case "^":
                case "log":
                    return true;
                default:
                    return false;
            }
        }
    }
}
