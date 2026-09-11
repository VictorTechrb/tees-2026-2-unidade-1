using Core;
using Core.Repository;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ItemAcervoRepository : IItemAcervoRepository{
  private readonly BibliotecaContext _context;

  public ItemAcervoRepository(BibliotecaContext context) {
    _context = context;
  }

  public uint Create(Itemacervo itemAcervo){
    _context.Add(itemAcervo);
    _context.SaveChanges();
    return itemAcervo.Id;
  }
  public void Edit(Itemacervo itemAcervo){
    _context.Update(itemAcervo);
    _context.SaveChanges();
  }
  public void Delete(int id){
    var itemAcervo = _context.ItensAcervo.Find(id);
    if (itemAcervo != null){
      _context.Remove(itemAcervo);
      _context.SaveChanges();
    }
  }
  public Itemacervo? Get(int id){
    return _context.ItensAcervo.Find(id);
  }
  public IEnumerable<ItemAcervoDto> GetAll() =>
    _context.ItensAcervo
        .AsNoTracking()
        .Select(i => new ItemAcervoDto
        {
            Id = i.Id,
            NomeBiblioteca = i.Biblioteca.Nome,
            NomeLivro = i.Livro.Nome,
            SituacaoItemAcervo = i.Situacao.ToString()
        });