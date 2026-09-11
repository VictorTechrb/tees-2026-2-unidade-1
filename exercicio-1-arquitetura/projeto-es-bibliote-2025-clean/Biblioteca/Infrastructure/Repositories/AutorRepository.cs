using Core;
using Core.Datatables;
using Core.DTO;
using Core.Repository;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories;
public class AutorRepository : IAutorRepository
{
    private readonly BibliotecaContext _context;
    public AutorRepository(BibliotecaContext context)
    {
        _context = context;
    }
    public uint Create(Autor autor)
    {
        _context.Add(autor);
        _context.SaveChanges();
        return autor.Id;
    }
    public void Edit(Autor autor)
    {
        _context.Update(autor);
        _context.SaveChanges();
    }
    public void Delete(uint id)
    {
        var autor = _context.Autors.Find(id);
        if (autor != null)
        {
            _context.Remove(autor);
            _context.SaveChanges();
        }
    }
    public Autor? Get(uint id) => _context.Autors.Find(id);
    public IEnumerable<Autor> GetAll() => _context.Autors.AsNoTracking();
    public IEnumerable<Autor> GetAllOrderByNome()
    {
        return _context.Autors
            .OrderByDescending(a => a.Nome)
            .AsNoTracking();
    }
    public int GetCountAutores() => _context.Autors.Count();
    public IEnumerable<Autor> GetByName(string nomeAutor)
    {
        return _context.Autors
            .Where(a => a.Nome.Contains(nomeAutor))
            .AsNoTracking()
            .ToList();
    }
    public IEnumerable<Autor> GetOrderByDescending()
    {
        return _context.Autors.OrderByDescending(a => a.Nome);
    }
    public IEnumerable<AutorDto> GetByNome(string nome)
    {
        return _context.Autors
            .Where(a => a.Nome.StartsWith(nome))
            .OrderBy(a => a.Nome)
            .Select(a => new AutorDto { Id = a.Id, Nome = a.Nome })
            .AsNoTracking();
    }
    public DatatableResponse<Autor> GetDataPage(DatatableRequest request)
    {
        var autores = _context.Autors.AsNoTracking();
        var totalRecords = autores.Count();
        if (request.Search != null && request.Search.GetValueOrDefault("value") != null)
        {
            var searchValue = request.Search.GetValueOrDefault("value") ?? string.Empty;
            autores = autores.Where(a => a.Id.ToString().Contains(searchValue)
                                      || a.Nome.ToLower().Contains(searchValue));
        }
        if (request.Order != null && request.Order[0].GetValueOrDefault("column").Equals("0"))
        {
            autores = request.Order[0].GetValueOrDefault("dir").Equals("asc")
                ? autores.OrderBy(a => a.Id)
                : autores.OrderByDescending(a => a.Id);
        }
        else if (request.Order != null && request.Order[0].GetValueOrDefault("column").Equals("1"))
        {
            autores = request.Order[0].GetValueOrDefault("dir").Equals("asc")
                ? autores.OrderBy(a => a.Nome)
                : autores.OrderByDescending(a => a.Nome);
        }
        else if (request.Order != null && request.Order[0].GetValueOrDefault("column").Equals("2"))
        {
            autores = request.Order[0].GetValueOrDefault("dir").Equals("asc")
                ? autores.OrderBy(a => a.DataNascimento)
                : autores.OrderByDescending(a => a.DataNascimento);
        }
        int countRecordsFiltered = autores.Count();
        autores = autores.Skip(request.Start).Take(request.Length);
        return new DatatableResponse<Autor>
        {
            Data = autores.ToList(),
            Draw = request.Draw,
            RecordsFiltered = countRecordsFiltered,
            RecordsTotal = totalRecords
        };
    }
}