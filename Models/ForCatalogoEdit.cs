namespace FerramentariaTest.Models
{
    public class ForCatalogoEdit
    {
     
        public int Id { get; set; }

    
        public int? IdCategoria { get; set; }

    
        public string? Codigo { get; set; }

   
        public string? Nome { get; set; }

    
        public string? Descricao { get; set; }

 
        public int? PorMetro { get; set; }

 
        public int? PorAferido { get; set; }


        public int? PorSerial { get; set; }

        public int? RestricaoEmprestimo { get; set; }

    
        public int? ImpedirDescarte { get; set; }

        public int? HabilitarDescarteEPI { get; set; }


        public int? DataDeRetornoAutomatico { get; set; }

        public DateTime? DataRegistro { get; set; }

        public int? Ativo { get; set; }

    }
}
