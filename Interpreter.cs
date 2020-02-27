using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

namespace KizhiPart2
{
    public class Interpreter
    {
        //asd 

        string command;//команда переданная интерпретатору

        InterpreterCommands itprCommands;

        enum codeRecordFlags
        {
            RecordDenied,
            RecordStarted,
            RecordStoped
        }
        codeRecordFlags recordFlag = codeRecordFlags.RecordDenied;

        public List<string> allCode = new List<string>();

        private TextWriter _writer;
        
        private Dictionary<string, int> bank = new Dictionary<string, int>();
        
        string[] commands;

        public Interpreter(TextWriter writer)
        {
            _writer = writer;
            itprCommands = new InterpreterCommands(bank,_writer,this);
        }

        public void ExecuteLine(string command)
        {
            
            try
            {
                commands = Parser.SplitCommand(command);
            }
            catch(ArgumentException msg)
            {
                _writer.WriteLine(msg.Message);
                return;
            }
            if(recordFlag==codeRecordFlags.RecordStoped & commands.SequenceEqual(InterpreterCommands.run))
            {
                for(int i=0;i<allCode.Count;i++)
                {
                    ExecuteLine(allCode[i]);
                }
                return;
            }
            if (recordFlag == codeRecordFlags.RecordStarted &commands.SequenceEqual(InterpreterCommands.endSetCodeString))
            {
                recordFlag = codeRecordFlags.RecordStoped;
                return;
            }
            if (recordFlag == codeRecordFlags.RecordStarted & !commands.SequenceEqual(InterpreterCommands.endSetCodeString))
            {
                commands = Parser.SplitBlocCommand(command);
                foreach(string cmd in commands)
                {
                    allCode.Add(cmd);
                }
                return;
            }
             
                if (commands.SequenceEqual(InterpreterCommands.setCodeString) & recordFlag==codeRecordFlags.RecordDenied)
            {
                recordFlag = codeRecordFlags.RecordStarted;
                return;
            }

            if (recordFlag==codeRecordFlags.RecordDenied)
            {
                _writer.WriteLine("Обозначьте начало ввода кода");
                return;
            }
           


            

            try
            {
                if (commands[commands.Length-1] == Parser.funDefMarker.TrimStart(' ')) itprCommands.DefineFunction( command);
                else itprCommands.commandList[commands[0]](commands);
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                _writer.WriteLine("Неизвестная команда (stop для выхода)");
            }

        }
        
    }
   
    public  class Parser
    {
        static public  string funDefMarker = " fundef";
        static public string[] SplitCommand(string command)
        {
            try
            {
                if (command.Substring(0, 4) == "    ")
                {
                    if (InterpreterCommands.isFunctionDefining)
                    {
                        command = command + funDefMarker;
                    }
                    else throw new ArgumentException("Функция не обьявлена для определения");
                }//добавляем в строку информацию
                 // о том что мы определяем функцию LastFunctionName
            }
            catch
            {

            }

            while (command.Contains("  "))//удаляем лишние пробелы
            {
                command = command.Replace("  ", " ");
            }
            command = command.Trim(' ');
            return command.Split(' ');

        }

        //разделяет блок кода   на отдельные строки кода
        static public string[] SplitBlocCommand(string command)
        {
            return command.Split('\n');
        }
    }

    public class InterpreterCommands
    {
        Dictionary<string, int> bank;
        TextWriter writer;
        Interpreter itpr;

        static public string[] setCodeString = { "set", "code" };
        static public string[] endSetCodeString = { "end", "set", "code" };
        static public string[] run = { "run" };
       


        public  delegate void ItprCommands(string[] command);

        //Инициализация в методе InitCommandDictionary
        public  Dictionary<string, ItprCommands> commandList = new Dictionary<string, ItprCommands>();


        public Dictionary<string, List<string>> functionList = new Dictionary<string, List<string>>();

        static private string _LastFunctionName; 

        static public bool isFunctionDefining { get { if (_LastFunctionName == null) return false; else return true; } }

        public InterpreterCommands(Dictionary<string, int> bank,TextWriter _writer,Interpreter itpr)
        {
            this.itpr = itpr;
            this.writer = _writer;
            this.bank = bank;
            InitCommandDictionary();
        }
        
        public void DefineFunction(string command)
        {
            functionList[_LastFunctionName].Add(command);
        }

        private void SetVar(string[] commands)
        {

            try
            {
               
                bank.Add(commands[1], System.Convert.ToInt16(commands[2]));

            }
            catch (System.ArgumentException msg)
            {
                Console.WriteLine(msg.Message);
            }
        }

        private void PrintVar(string[] commands)
        {
            try
            {
                writer.WriteLine(bank[commands[1]]);
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                writer.WriteLine("Переменная отсутствует в памяти");
            }
        }

        private void SubVar(string[] commands)
        {
            try
            {
                bank[commands[1]] = bank[commands[1]] - Convert.ToInt16(commands[2]);
            }
            catch
            {
                writer.WriteLine("Переменная отсутствует в памяти");
            }
        }

        private void RemVar(string[] commands)
        {
            if (bank.ContainsKey(commands[1]))
            {
                bank.Remove(commands[1]);
            }

            else writer.WriteLine("Переменная отсутствует в памяти");

        }

        private void DefineFunName(string[] commands)
        {
            try
            {
                functionList.Add(commands[1], new List<string>());
                _LastFunctionName = commands[1];
            }
            catch (ArgumentException msg)
            {
                writer.WriteLine(msg.Message);
            }
        }

        private void CallFunction(string[] commands)
        {
            
                for(int i =0;i<functionList[commands[1]].Count;i++)
            {
                itpr.ExecuteLine(functionList[commands[1]][i].Trim(' '));
            }
            
        }

        private void InitCommandDictionary()
        {
            commandList.Add("set", SetVar);
            commandList.Add("print", PrintVar);
            commandList.Add("sub", SubVar);
            commandList.Add("rem", RemVar);
            commandList.Add("def", DefineFunName);
            commandList.Add("call", CallFunction);
            
            
        }


    }

   
    
}