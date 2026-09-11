using Core.DTO;

namespace Core.Repository;

public interface IItemAcervoRepository
{
    uint Create(Itemacervo itemAcervo);
    void Edit(Itemacervo itemAcervo);
    void Delete(int id);
    Itemacervo? Get(int id);
    IEnumerable<ItemAcervoDto> GetAll();
}
