using Microsoft.EntityFrameworkCore;
using PokerMetricsCore.Web.Models;

namespace PokerMetricsCore.Web.Data
{
    public class PokerMetricsCoreContext : DbContext
    {
        public PokerMetricsCoreContext(DbContextOptions<PokerMetricsCoreContext> options)
            : base(options)
        {
        }

        public DbSet<Player> Player { get; set; }
        public DbSet<DefinicaoTorneio> DefinicaoTorneio { get; set; }
        public DbSet<Arquivo> Arquivo { get; set; }
        public DbSet<Transacao> Transacao { get; set; }
        public DbSet<TorneioJogado> TorneioJogado { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Player>(entity =>
            {
                entity.ToTable("PLAYER");
                entity.HasKey(p => p.PlayerId);
                entity.Property(p => p.Name)
                     .HasMaxLength(100)
                     .IsRequired(); // NOT NULL
            });

            modelBuilder.Entity<DefinicaoTorneio>(entity =>
            {
                entity.ToTable("DEFINICAO_TORNEIO");
                entity.HasKey(t => t.DefinicaoTorneioId);
                entity.Property(t => t.Nome)
                     .HasMaxLength(255)
                     .IsRequired();
                
                entity.Property(t => t.BuyIn)
                     .HasColumnType("decimal(18, 2)")
                     .IsRequired();
            });

            modelBuilder.Entity<Arquivo>(entity =>
            {
                entity.ToTable("ARQUIVO");
                entity.HasKey(f => f.ArquivoId);
                entity.Property(f => f.NomeOriginalArquivo)
                     .HasMaxLength(300)
                     .IsRequired();
                entity.Property(f => f.DataUploadUtc).IsRequired();
                entity.Property(f => f.PeriodStartDate).IsRequired();
                entity.Property(f => f.PeriodEndDate).IsRequired();

                entity.HasOne(f => f.Player) // Um arquivo processado tem um Player
                     .WithMany(p => p.Arquivos) // Um Player tem muitos arquivos
                     .HasForeignKey(f => f.PlayerId)
                     .OnDelete(DeleteBehavior.Cascade); // Se um Player for deletado, seus arquivos são deletados
            });

            modelBuilder.Entity<Transacao>(entity =>
            {
                entity.ToTable("TRANSACAO");
                entity.HasKey(t => t.TransacaoId);
                entity.Property(t => t.Descricao)
                     .HasMaxLength(255)
                     .IsRequired();
                
                entity.Property(t => t.ReferenceId) // ReferenceId pode ser nulo 
                     .HasMaxLength(50) 
                     .IsRequired(false); 

                entity.Property(t => t.ValorMonetario)
                     .HasColumnType("decimal(18, 2)")
                     .IsRequired();
                
                entity.Property(t => t.Pontos)
                     .HasColumnType("decimal(10, 2)")
                     .IsRequired();

                entity.HasOne(t => t.Arquivo)
                     .WithMany(f => f.Transacoes)
                     .HasForeignKey(t => t.ArquivoId)
                     .OnDelete(DeleteBehavior.Cascade);  // Se um arquivo for deletado, suas transações são deletadas
            });

            modelBuilder.Entity<TorneioJogado>(entity =>
            {
                entity.ToTable("TORNEIO_JOGADO");
                entity.HasKey(pt => pt.TorneioJogadoId);

                entity.Property(pt => pt.ReferenceId)
                     .HasMaxLength(50)
                     .IsRequired();
                
                entity.HasIndex(pt => pt.ReferenceId);

                entity.Property(pt => pt.TotalBuyIn)
                     .HasColumnType("decimal(18, 2)")
                     .IsRequired();
                
                entity.Property(pt => pt.TotalPayout)
                     .HasColumnType("decimal(18, 2)")
                     .IsRequired();
                
                entity.Property(pt => pt.NetResult)
                     .HasColumnType("decimal(18, 2)")
                     .IsRequired();

                entity.HasOne(pt => pt.Player)
                     .WithMany(p => p.TorneiosJogados)
                     .HasForeignKey(pt => pt.PlayerId)
                     .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pt => pt.DefinicaoTorneio)
                     .WithMany() // Sem propriedade de navegação de volta
                     .HasForeignKey(pt => pt.DefinicaoTorneioId)
                     .OnDelete(DeleteBehavior.Restrict); // Impede que uma definição seja deletada se houver torneios jogados ligados a ela
            });

            // Data Seeding para carregar os dados da GRADE MAX LATE no banco
            modelBuilder.Entity<DefinicaoTorneio>().HasData(
            // Torneios de $5.50
            new DefinicaoTorneio { DefinicaoTorneioId = 1, Nome = "MICRO ROLLER", BuyIn = 5.50m, HorarioComeco = new TimeOnly(23, 34) },
            new DefinicaoTorneio { DefinicaoTorneioId = 2, Nome = "MICRO NIGHTLY", BuyIn = 5.50m, HorarioComeco = new TimeOnly(1, 54) },
          
            // Torneios de $11.00
            new DefinicaoTorneio { DefinicaoTorneioId = 3, Nome = "LUCKY 7s", BuyIn = 11.00m, HorarioComeco = new TimeOnly(0, 20) },
            new DefinicaoTorneio { DefinicaoTorneioId = 4, Nome = "CRAZY 8s", BuyIn = 11.00m, HorarioComeco = new TimeOnly(14, 30) },
            new DefinicaoTorneio { DefinicaoTorneioId = 5, Nome = "CRAZY KO", BuyIn = 11.00m, HorarioComeco = new TimeOnly(23, 10) },
          
            // Torneios de $16.50
            new DefinicaoTorneio { DefinicaoTorneioId = 6, Nome = "LOW ROLLER", BuyIn = 16.50m, HorarioComeco = new TimeOnly(22, 34) },
            new DefinicaoTorneio { DefinicaoTorneioId = 7, Nome = "CRAZY 8s MADRUGADA", BuyIn = 16.50m, HorarioComeco = new TimeOnly(2, 10) },
          
            // Torneios de $9.90
            new DefinicaoTorneio { DefinicaoTorneioId = 8, Nome = "MINOR NINER", BuyIn = 9.90m, HorarioComeco = new TimeOnly(3, 40) },
            new DefinicaoTorneio { DefinicaoTorneioId = 9, Nome = "EARLY NINER", BuyIn = 9.90m, HorarioComeco = new TimeOnly(15, 30) },
          
            // Torneios de $4.40
            new DefinicaoTorneio { DefinicaoTorneioId = 10, Nome = "MICRO MONSTER", BuyIn = 4.40m, HorarioComeco = new TimeOnly(1, 10) },
          
            // Torneios de $55.00
            new DefinicaoTorneio { DefinicaoTorneioId = 11, Nome = "NIGHTLY", BuyIn = 55.00m, HorarioComeco = new TimeOnly(2, 30) },
          
            // Free Rolls
            new DefinicaoTorneio { DefinicaoTorneioId = 12, Nome = "FREEROLL", BuyIn = 0.00m, HorarioComeco = new TimeOnly(0, 0) }
            );
          }
     }
}