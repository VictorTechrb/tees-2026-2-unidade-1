using Core;
using Core.Datatables;
using Core.DTO;
using Core.Ports.Entrada;
using Core.Ports.Saida;
using Core.Service;

namespace Application
{
    /// <summary>
    /// Implementa serviços para manter dados do autor.
    /// Vive no interior do hexágono: conhece a porta de entrada que atende
    /// (<see cref="IAutorService"/>) e a porta de saída de que precisa
    /// (<see cref="IAutorRepositorioPort"/>), mas não conhece Entity Framework,
    /// banco de dados nem qualquer detalhe de persistência.
    /// </summary>
    public class AutorService : IAutorService
    {
        private readonly IAutorRepositorioPort repositorio;

        public AutorService(IAutorRepositorioPort repositorio)
        {
            this.repositorio = repositorio;
        }

        /// <summary>
        /// Criar um novo autor na base de dados
        /// </summary>
        /// <param name="autor">dados do autor</param>
        /// <returns>id do autor</returns>
        /// <exception cref="ServiceException"></exception>
        public uint Create(Autor autor)
        {
            ValidarDataNascimento(autor);
            return repositorio.Create(autor);
        }

        /// <summary>
        /// Editar dados do autor na base de dados
        /// </summary>
        /// <param name="autor"></param>
        /// <exception cref="ServiceException"></exception>
        public void Edit(Autor autor)
        {
            ValidarDataNascimento(autor);
            repositorio.Edit(autor);
        }

        /// <summary>
        /// Remover o autor da base de dados
        /// </summary>
        /// <param name="id">id do autor</param>
        public void Delete(uint id)
        {
            repositorio.Delete(id);
        }

        /// <summary>
        /// Buscar um autor na base de dados
        /// </summary>
        /// <param name="id">id autor</param>
        /// <returns>dados do autor</returns>
        public Autor? Get(uint id)
        {
            return repositorio.Get(id);
        }

        /// <summary>
        /// Buscar todos os autores cadastrados
        /// </summary>
        /// <returns>lista de autores</returns>
        public IEnumerable<Autor> GetAll()
        {
            return repositorio.GetAll();
        }

        /// <summary>
        /// Buscar autores iniciando com o nome
        /// </summary>
        /// <param name="nome">nome do autor</param>
        /// <returns>lista de autores que inicia com o nome</returns>
        public IEnumerable<AutorDto> GetByNome(string nome)
        {
            return repositorio.GetByNome(nome);
        }

        /// <summary>
        /// Retorna uma página de dados
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DatatableResponse<Autor> GetDataPage(DatatableRequest request)
        {
            return repositorio.GetDataPage(request);
        }

        /// <summary>
        /// Regra de negócio do agregado Autor, preservada do serviço original.
        /// É a única lógica que não pode migrar para o adaptador de persistência.
        /// </summary>
        /// <exception cref="ServiceException"></exception>
        private static void ValidarDataNascimento(Autor autor)
        {
            if (autor.DataNascimento.Year < 1000)
                throw new ServiceException("O ano de nascimento de autor deve ser maior do que 1000. Favor informar nova data.");
        }
    }
}
