using CentroSenderos_2026_BD;
using System.Linq.Expressions;

namespace Modelado2025_1Repositorio.Repositorios
{
    public interface IRepositorio<E> where E : class, IEntityBase
    {
        Task<bool> Existe(int id);
        Task<int> Insert(E entidad);
        Task<List<E>> Select();
        Task<E?> SelectById(int id);
        Task<bool> Update(int id, E entidad);
        Task<bool> Delete(int id);
        Task<bool> ExistePredi(Expression<Func<E, bool>> predicate);
    }
}