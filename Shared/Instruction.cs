using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderBackup.Shared
{
    [Serializable()]
    public class Instruction 
    {
        public InstructionType cmd;
        public string src, dst;

        public Instruction(InstructionType type, string op1, string op2)
        {
            this.cmd = type;
            this.src = op1;
            this.dst = op2;
        }
    }

    public enum InstructionType
    {
        COPY, NEW, DELETE
    }
}
