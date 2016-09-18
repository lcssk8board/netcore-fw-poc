using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFCore.Tests
{
    public class Enumerators
    {
        /// <summary>
        /// Enum responsável por pontos de referência para configuração de qual ORM a DAO utilizará.
        /// </summary>
        public enum RepositoryType : byte
        {
            EF6 = 0,
            NHabernate = 1,
            ADO = 2,
            TextFile = 3,
            XmlFile = 4,
            EntityFrameworkCore = 5,
        }
    }
}
