using System.Collections.Generic;

namespace Elecciones_Europeas.src.service
{
    internal interface IBaseService<T, ID>
    {
        T FindById(ID id);
        List<T> FindAll();
    }
}
