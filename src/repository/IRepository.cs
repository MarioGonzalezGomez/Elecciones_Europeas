using Elecciones_Europeas.src.model.IPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elecciones_Europeas.src.repository
{
    internal interface IRepository<T,ID>
    {
        public List<T> GetAll();
        public T GetById(ID id);
    }
}
