using Core.DTO;

namespace Core.Repository;

public interface IItemAcervoRepository
{
    uint Create(Itemacervo itemAcervo);
    void Edit(Itemacervo itemAcervo);
    void Delete(uint id);
    Task<Itemacervo?> Get(uint id);
    Task<IEnumerable<ItemAcervoDto>> GetAll();
}
