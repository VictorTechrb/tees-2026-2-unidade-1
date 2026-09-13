using Core;
using Core.DTO;
using Core.Repository;
using Core.Service;

namespace Service;

public class ItemAcervoService : IItemAcervoService
{
    private readonly IItemAcervoRepository _itemAcervoRepository;
    public ItemAcervoService(IItemAcervoRepository itemAcervoRepository) 
    { 
        _itemAcervoRepository = itemAcervoRepository; 
    }

    public uint Create(Itemacervo itemAcervo) => _itemAcervoRepository.Create(itemAcervo);
    public void Edit(Itemacervo itemAcervo) => _itemAcervoRepository.Edit(itemAcervo);
    public void Delete(int id) => _itemAcervoRepository.Delete((uint)id);
    public Itemacervo? Get(int id) => _itemAcervoRepository.Get((uint)id).Result;
    public IEnumerable<ItemAcervoDto> GetAll() => _itemAcervoRepository.GetAll().Result;
}
