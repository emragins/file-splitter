﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileSplitter
{
    public class FileSizeContext
    {
        private string input;
        private long output;

        public FileSizeContext(string input)
        {
            this.Input = input;
        }

        public string Input { get; set; }

        public long Output { get; set; }
    }

    public abstract class FileSizeExpression
    {
        public abstract void Interpret(FileSizeContext value);
    }

    //Then you define your terminal expression,a nd all of the variants:

    public abstract class TerminalFileSizeExpression : FileSizeExpression
    {
        public override void Interpret(FileSizeContext value)
        {
            if (value.Input.EndsWith(this.ThisPattern()))
            {
                double amount = double.Parse(value.Input.Replace(this.ThisPattern(), String.Empty));
                var fileSize = (long)(amount * 1024);
                value.Input = String.Format("{0}{1}", fileSize, this.NextPattern());
                value.Output = fileSize;
            }
        }
        protected abstract string ThisPattern();
        protected abstract string NextPattern();
    }

    public class KbFileSizeExpression : TerminalFileSizeExpression
    {
        protected override string ThisPattern() { return "KB"; }
        protected override string NextPattern() { return "bytes"; }
    }
    public class MbFileSizeExpression : TerminalFileSizeExpression
    {
        protected override string ThisPattern() { return "MB"; }
        protected override string NextPattern() { return "KB"; }
    }
    public class GbFileSizeExpression : TerminalFileSizeExpression
    {
        protected override string ThisPattern() { return "GB"; }
        protected override string NextPattern() { return "MB"; }
    }
    public class TbFileSizeExpression : TerminalFileSizeExpression
    {
        protected override string ThisPattern() { return "TB"; }
        protected override string NextPattern() { return "GB"; }
    }

    ///Then you add a non-terminal expression (this does the bulk of the work):

    public class FileSizeParser : FileSizeExpression
    {
        private List<FileSizeExpression> expressionTree = new List<FileSizeExpression>()
                                                  {
                                                      new TbFileSizeExpression(),
                                                      new GbFileSizeExpression(),
                                                      new MbFileSizeExpression(),
                                                      new KbFileSizeExpression()
                                                  };

        public override void Interpret(FileSizeContext value)
        {
            foreach (FileSizeExpression exp in expressionTree)
            {
                exp.Interpret(value);
            }
        }
    }

    //Finally, here is the sort of client code:

}
