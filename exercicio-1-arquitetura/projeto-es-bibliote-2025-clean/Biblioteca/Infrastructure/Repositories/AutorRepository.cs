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
    public async Task<Autor?> Get(uint id){
        return  await _context.Autors.FindAsync(id);
        }
    public async Task<IEnumerable<Autor>> GetAll() { 
        return await _context.Autors.AsNoTracking().ToListAsync();
    }
    public async Task<IEnumerable<Autor>> GetAllOrderByNome()
    {
        return await _context.Autors
            .OrderByDescending(a => a.Nome)
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<int> GetCountAutores() { 
        return await _context.Autors.
        CountAsync();
    }
    public async Task<IEnumerable<Autor>> GetByName(string nomeAutor)
    {
        return await _context.Autors
            .Where(a => a.Nome.Contains(nomeAutor))
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<IEnumerable<Autor>> GetOrderByDescending()
    {
        return await _context.Autors.
        OrderByDescending(a => a.Nome)
        .AsNoTracking()
        .ToListAsync();
    }
    public async Task<IEnumerable<AutorDto>> GetByNome(string nome)
    {
        return await _context.Autors
            .Where(a => a.Nome.StartsWith(nome))
            .OrderBy(a => a.Nome)
            .Select(a => new AutorDto { Id = a.Id, Nome = a.Nome })
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<DatatableResponse<Autor>> GetDataPage(DatatableRequest request)
    {
        var autores = _context.Autors.AsNoTracking();
        var totalRecords = await autores.CountAsync();

        if (request.Search != null && request.Search.GetValueOrDefault("value") != null)
        {
            var searchValue = (request.Search.GetValueOrDefault("value") ?? string.Empty).ToLower();
            autores = autores.Where(a => a.Id.ToString().Contains(searchValue)
                                    || a.Nome.ToLower().Contains(searchValue));
        }

        if (request.Order != null && request.Order.Count > 0 && request.Order[0].GetValueOrDefault("column").Equals("0"))
        {
            autores = request.Order[0].GetValueOrDefault("dir").Equals("asc")
                ? autores.OrderBy(a => a.Id)
                : autores.OrderByDescending(a => a.Id);
        }
        else if (request.Order != null && request.Order.Count > 0 && request.Order[0].GetValueOrDefault("column").Equals("1"))
        {
            autores = request.Order[0].GetValueOrDefault("dir").Equals("asc")
                ? autores.OrderBy(a => a.Nome)
                : autores.OrderByDescending(a => a.Nome);
        }
        else if (request.Order != null && request.Order.Count > 0 && request.Order[0].GetValueOrDefault("column").Equals("2"))
        {
            autores = request.Order[0].GetValueOrDefault("dir").Equals("asc")
                ? autores.OrderBy(a => a.DataNascimento)
                : autores.OrderByDescending(a => a.DataNascimento);
        }

        int countRecordsFiltered = await autores.CountAsync();
        autores = autores.Skip(request.Start).Take(request.Length);

        return new DatatableResponse<Autor>
        {
            Data = await autores.ToListAsync(),
            Draw = request.Draw,
            RecordsFiltered = countRecordsFiltered,
            RecordsTotal = totalRecords
        };
    }
}