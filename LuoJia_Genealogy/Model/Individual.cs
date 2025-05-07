using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuoJia_Genealogy.Model
{
    [Table("home_individual")]
    public class Individual
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("create_time")]
        public DateTime CreateTime { get; set; }

        [Required]
        [Column("update_time")]
        public DateTime UpdateTime { get; set; }

        [Required]
        [Column("surname", TypeName = "varchar(10)")]
        public string Surname { get; set; }

        [Column("name", TypeName = "varchar(10)")]
        public string? Name { get; set; }

        [Column("zi", TypeName = "varchar(10)")]
        public string? Zi { get; set; }

        [Column("hao", TypeName = "varchar(10)")]
        public string? Hao { get; set; }

        [Column("common_name", TypeName = "varchar(10)")]
        public string? CommonName { get; set; }

        [Required]
        [Column("gender", TypeName = "varchar(1)")]
        public string Gender { get; set; }

        [Column("ad_birth")]
        public DateTime? AdBirth { get; set; }

        [Column("ce_birth", TypeName = "varchar(32)")]
        public string? CeBirth { get; set; }

        [Column("birth_place", TypeName = "varchar(50)")]
        public string? BirthPlace { get; set; }

        [Column("ad_death")]
        public DateTime? AdDeath { get; set; }

        [Column("ce_death", TypeName = "varchar(32)")]
        public string? CeDeath { get; set; }

        [Column("death_place", TypeName = "varchar(50)")]
        public string? DeathPlace { get; set; }

        [Column("line_name", TypeName = "varchar(2)")]
        public string? LineName { get; set; }

        [Column("generetion", TypeName = "varchar(2)")]
        public string? Generation { get; set; }

        [Column("rank", TypeName = "varchar(32)")]
        public string? Rank { get; set; }

        [Column("is_alive", TypeName = "varchar(1)")]
        public string? IsAlive { get; set; }

        [Column("biography", TypeName = "varchar(1000)")]
        public string? Biography { get; set; }

        [Column("epitaph", TypeName = "varchar(1000)")]
        public string? Epitaph { get; set; }

        [Column("idcard_no", TypeName = "varchar(18)")]
        public string? IdCardNo { get; set; }

        [Column("address", TypeName = "varchar(50)")]
        public string? Address { get; set; }

        [Column("phone", TypeName = "varchar(11)")]
        public string? Phone { get; set; }

        [Column("idcard_type", TypeName = "varchar(32)")]
        public string? IdCardType { get; set; }

        [Required]
        [Column("is_del", TypeName = "varchar(1)")]
        public string IsDel { get; set; }

        [Column("note", TypeName = "varchar(500)")]
        public string? Note { get; set; }

        [Column("create_by_id", TypeName = "varchar(32)")]
        public string? CreateById { get; set; }

        [Column("farther_id")]
        public long? FatherId { get; set; }

        [Column("gene_id", TypeName = "varchar(50)")]
        public string? GeneId { get; set; }

        [Column("mother_id")]
        public long? MotherId { get; set; }

        [Column("spouse_id")]
        public long? SpouseId { get; set; }

        [Column("update_by_id", TypeName = "varchar(32)")]
        public string? UpdateById { get; set; }

        [Column("cemetery", TypeName = "varchar(50)")]
        public string? Cemetery { get; set; }

        // 导航属性 - 父亲
        [ForeignKey("FatherId")]
        public virtual Individual? Father { get; set; }

        // 导航属性 - 母亲
        [ForeignKey("MotherId")]
        public virtual Individual? Mother { get; set; }

        // 导航属性 - 配偶
        [ForeignKey("SpouseId")]
        public virtual Individual? Spouse { get; set; }
    }
}

