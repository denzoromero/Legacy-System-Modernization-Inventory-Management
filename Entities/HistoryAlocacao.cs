using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Entities
{
    public class HistoricoAlocacao
    {
        [Key]
        public int? Id { get; set; }
        public int? IdProduto { get; set; }
        public int? Solicitante_IdTerceiro { get; set; }
        public int? Solicitante_CodColigada { get; set; }
        [MaxLength(10)]
        public string? Solicitante_Chapa { get; set; }
        public int? Liberador_IdTerceiro { get; set; }
        public int? Liberador_CodColigada { get; set; }
        [MaxLength(10)]
        public string? Liberador_Chapa { get; set; }
        public int? Balconista_Emprestimo_IdLogin { get; set; }
        public int? Balconista_Devolucao_IdLogin { get; set; }
        [MaxLength(250)]
        public string? Observacao { get; set; }
        public DateTime? DataEmprestimo { get; set; }
        public DateTime? DataPrevistaDevolucao { get; set; }
        public DateTime? DataDevolucao { get; set; }
        public int? IdObra { get; set; }
        public int? Quantidade { get; set; }
        public int? IdFerrOndeProdRetirado { get; set; }
        public int? IdFerrOndeProdDevolvido { get; set; }
        [Precision(16,2)]
        public decimal? Kilo { get; set; }
        public int? IdControleCA { get; set; }
        public int? IdReservation { get; set; }
        [MaxLength(255)]
        public string? TransactionId { get; set; }
        [MaxLength(255)]
        public string? EmprestimoTransactionId { get; set; }
        [MaxLength(255)]
        public string? CrachaNo { get; set; }
    }
    public class HistoricoAlocacao_2000 : HistoricoAlocacao
    {
        // Specific properties for the year 2016
    }
    public class HistoricoAlocacao_2001 : HistoricoAlocacao
    {
        // Specific properties for the year 2016
    }
    public class HistoricoAlocacao_2002 : HistoricoAlocacao
    {
        // Specific properties for the year 2016
    }

    public class HistoricoAlocacao_2003 : HistoricoAlocacao
    {
        // Specific properties for the year 2016
    }

    public class HistoricoAlocacao_2004 : HistoricoAlocacao
    {
        // Specific properties for the year 2016
    }

    public class HistoricoAlocacao_2005 : HistoricoAlocacao
    {
        // Specific properties for the year 2016
    }

    public class HistoricoAlocacao_2006 : HistoricoAlocacao
    {
        // Specific properties for the year 2016
    }

    public class HistoricoAlocacao_2007 : HistoricoAlocacao
    {
        // Specific properties for the year 2016
    }

    public class HistoricoAlocacao_2008 : HistoricoAlocacao
    {
        // Specific properties for the year 2016
    }

    public class HistoricoAlocacao_2009 : HistoricoAlocacao
    {
        // Specific properties for the year 2016
    }

    public class HistoricoAlocacao_2010 : HistoricoAlocacao
    {
        // Specific properties for the year 2016
    }

    public class HistoricoAlocacao_2011 : HistoricoAlocacao
    {
        // Specific properties for the year 2016
    }

    public class HistoricoAlocacao_2012 : HistoricoAlocacao
    {
        // Specific properties for the year 2016
    }

    public class HistoricoAlocacao_2013 : HistoricoAlocacao
    {
        // Specific properties for the year 2016
    }

    public class HistoricoAlocacao_2014 : HistoricoAlocacao
    {
        // Specific properties for the year 2016
    }

    public class HistoricoAlocacao_2015 : HistoricoAlocacao
    {
        // Specific properties for the year 2016
    }

    public class HistoricoAlocacao_2016 : HistoricoAlocacao
    {
        // Specific properties for the year 2016
    }

    public class HistoricoAlocacao_2017 : HistoricoAlocacao
    {
        // Specific properties for the year 2017
    }

    public class HistoricoAlocacao_2018 : HistoricoAlocacao
    {
       
    }

    public class HistoricoAlocacao_2019 : HistoricoAlocacao
    {

    }

    public class HistoricoAlocacao_2020 : HistoricoAlocacao
    {

    }

    public class HistoricoAlocacao_2021 : HistoricoAlocacao
    {

    }

    public class HistoricoAlocacao_2022 : HistoricoAlocacao
    {

    }

    public class HistoricoAlocacao_2023 : HistoricoAlocacao
    {

    }

    public class HistoricoAlocacao_2024 : HistoricoAlocacao
    {

    }

    public class HistoricoAlocacao_2025 : HistoricoAlocacao
    {
      
    }

}
