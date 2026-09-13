using Core;
using Core.Datatables;
using Core.DTO;
using Core.Ports.Saida;

// Nenhum using de Entity Framework neste arquivo — e esse é exatamente o ponto.
// Se a regra de negócio precisasse do EF, este adaptador não seria possível.

namespace Application.Tests.Fakes
{
    /// <summary>
    /// Segundo adaptador de saída (driven) da porta <see cref="IAutorRepositorioPort"/>,
    /// guardando os autores em uma lista na memória.
    /// Existe para provar que o hexágono não depende de Entity Framework: a mesma porta
    /// aceita tanto o adaptador de produção quanto este.
    /// </summary>
    public class AutorRepositorioEmMemoria : IAutorRepositorioPort
    {
        private readonly List<Autor> autores = new();

        public uint Create(Autor autor)
        {
            autores.Add(autor);
            return autor.Id;
        }

        public void Edit(Autor autor)
        {
            var indice = autores.FindIndex(a => a.Id == autor.Id);
            if (indice >= 0)
                autores[indice] = autor;
        }

        public void Delete(uint id)
        {
            autores.RemoveAll(a => a.Id == id);
        }

        public Autor? Get(uint id)
        {
            return autores.FirstOrDefault(a => a.Id == id);
        }

        public IEnumerable<Autor> GetAll()
        {
            return autores;
        }

        public IEnumerable<AutorDto> GetByNome(string nome)
        {
            return autores
                .Where(a => a.Nome.StartsWith(nome))
                .OrderBy(a => a.Nome)
                .Select(a => new AutorDto { Id = a.Id, Nome = a.Nome });
        }

        public DatatableResponse<Autor> GetDataPage(DatatableRequest request)
        {
            var total = autores.Count;
            var pagina = autores.Skip(request.Start).Take(request.Length).ToList();

            return new DatatableResponse<Autor>
            {
                Data = pagina,
                Draw = request.Draw,
                RecordsFiltered = total,
                RecordsTotal = total
            };
        }
    }
}
