using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFCore.Tests
{
    /// <summary>
    /// Classe com o propósito de armazenar as informações úteis de uma Model mapeada.
    /// </summary>
    public class FormatProperty
    {
        /// <summary>
        /// Propriedade para indicar o nome de coluna vinculada
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Propriedade para indicar o nome da propriedade
        /// </summary>
        public string PropertyName { get; set; }
    }
}
