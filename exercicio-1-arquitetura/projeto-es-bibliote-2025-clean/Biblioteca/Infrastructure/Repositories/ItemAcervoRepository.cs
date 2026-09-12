using Core;
using Core.DTO;
using Core.Repository;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ItemAcervoRepository : IItemAcervoRepository
{
  private readonly BibliotecaContext _context;

  public ItemAcervoRepository(BibliotecaContext context) 
  {
    _context = context;
  }

  public uint Create(Itemacervo itemAcervo)
  {
    _context.Add(itemAcervo);
    _context.SaveChanges();
    return itemAcervo.Id;
  }

  public void Edit(Itemacervo itemAcervo)
  {
    _context.Update(itemAcervo);
    _context.SaveChanges();
  }

  public void Delete(uint id)
  {
    var itemAcervo = _context.Itemacervos.Find(id);
    if (itemAcervo != null)
    {
      _context.Remove(itemAcervo);
      _context.SaveChanges();
    }
  }

  public async Task<Itemacervo?> Get(uint id)
  {
    return await _context.Itemacervos.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
  }

  public async Task<IEnumerable<ItemAcervoDto>> GetAll() 
  {
    var query = from item in _context.Itemacervos
                orderby item.IdLivroNavigation.Nome ascending 
                select new ItemAcervoDto
                {
                  Id = item.Id,
                  NomeBiblioteca = item.IdBibliotecaNavigation.Nome,
                  NomeLivro = item.IdLivroNavigation.Nome,
                  SituacaoItemAcervo = item.IdSituacaoItemAcervoNavigation.Situacao
                };
    return await query.AsNoTracking().ToListAsync();
  }
}