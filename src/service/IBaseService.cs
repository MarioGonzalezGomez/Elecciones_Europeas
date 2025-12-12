using System.Collections.Generic;

namespace Elecciones.src.service
{
    internal interface IBaseService<T, ID>
    {
        T FindById(ID id);
        List<T> FindAll();
    }
}
