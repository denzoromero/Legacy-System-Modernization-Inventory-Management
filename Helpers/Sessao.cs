using Newtonsoft.Json;

namespace FerramentariaTest.Helpers
{
    public class Sessao
    {
        public static string ID = "ID_USUARIO";
        public static string CHAPA = "CHAPA_USUARIO";
        public static string NOME = "NOME_FUNCIONARIO";
        public static string LOGADO = "LOGADO";
        public static string SAMACCOUNT = "SAMACCOUNT";
        public static string PAGINA = "PAGINA";

        public static string Environment = "Environment";

        public static string Solicitante = "Solicitante";
        public static string SolicitanteDevolucao = "SolicitanteDevolucao";
        public static string Liberador = "Liberador";
        public static string Ferramentaria = "Ferramentaria";
        public static string FerramentariaNome = "FerramentariaNome";

        public static string SupervisorId = "SupervisorId";
        public static string SupervisorChapa = "SupervisorChapa";

        public static string Funcionario = "Funcionario";

        public static string NomeFerramentaria = "NomeFerramentaria";
        public static string IdFerramentaria = "IdFerramentaria";

    }

    public static class SessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
