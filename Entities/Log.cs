namespace FerramentariaTest.Entities
{
    public class Log
    {
        public int Id { get; set; }
        public string? LogWhat { get; set; } // O que
        public string? LogWhy { get; set; } // por que
        public string? LogWhere { get; set; } // Onde - Local
        public string? LogWhen { get; set; } // Quando - Getdate
        public string? LogWho { get; set; } // Quem
        public string? LogHow { get; set; } // Como - Query
        public string? LogHowMuch { get; set; } // Quanto

    }
}
