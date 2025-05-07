using LuoJia_Genealogy.Data;
using LuoJia_Genealogy.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace LuoJia_Genealogy.Service
{
    public class IndividualService
    {
        private readonly IndividualDataContext _context;
        public IndividualService(IndividualDataContext context)
        {
            _context = context;
        }

        //获取指定家谱的所有成员信息
        private List<Individual> GetAllMembers(string geneId)
        {
            return _context.Individuals
                .Where(i => i.GeneId == geneId)
                .Include(i => i.Father)
                .Include(i => i.Mother)
                .Include(i => i.Spouse)
                .ToList();
        }

        private List<IndividualWithFamily> GetOrderedGenealogy(string geneId)
        {
            var allMember = GetAllMembers(geneId);
            var allMemberId = new HashSet<long>(allMember.Select(i => i.Id));

            var clanMembers = allMember
                            .Where(member => IsClanMember(member, allMemberId))
                            .ToList();

            // 构造家族树并计算正确世代
            var familyTree = BuildFamilyTree(clanMembers, allMemberId);
            var orderedClanMembers = new List<IndividualWithFamily>();

            // 修改后的层次遍历，会计算并修正世代
            LevelOrderTraversalWithGeneration(familyTree, orderedClanMembers);

            // 加入非本族配偶
            AddSpouses(orderedClanMembers, allMember);

            return orderedClanMembers;
        }

        //判断成员是否为本族成员
        private bool IsClanMember(Individual member, HashSet<long> allMemberIds)
        {
            // 有父母且在族谱中
            bool hasClanParent = (member.FatherId.HasValue && allMemberIds.Contains(member.FatherId.Value)) ||
                                (member.MotherId.HasValue && allMemberIds.Contains(member.MotherId.Value));

            // 无父母但符合先祖条件
            bool isFounder = !member.FatherId.HasValue &&
                            !member.MotherId.HasValue &&
                            member.Generation == "1" &&
                            member.Gender == "0";

            return hasClanParent || isFounder;
        }

        //构建家族树
        private TreeNode BuildFamilyTree(List<Individual> clanMembers, HashSet<long> allMemberIds)
        {
            // 创建所有节点
            var nodeDict = clanMembers.ToDictionary(
                m => m.Id,
                m => new TreeNode { Individual = m }
            );

            // 构建父子关系
            foreach (var member in clanMembers)
            {
                var currentNode = nodeDict[member.Id];

                // 添加子节点
                var children = clanMembers
                    .Where(m =>
                        (m.FatherId == member.Id && allMemberIds.Contains(member.Id)) ||
                        (m.MotherId == member.Id && allMemberIds.Contains(member.Id)))
                    .OrderBy(m => SafeParseRank(m.Rank))
                    .ToList();

                foreach (var child in children)
                {
                    currentNode.Children.Add(nodeDict[child.Id]);
                }
            }
            var roots = clanMembers
                    .Where(m => IsAncester(m))
                    .Select(m => nodeDict[m.Id])
                    .OrderBy(n => SafeParseRank(n.Individual.Rank))
                    .ToList();

            return roots.FirstOrDefault(); 
        }

        //遍历家族树
        private void LevelOrderTraversal(TreeNode root, List<IndividualWithFamily> result)
        {
            if (root == null) return;

            var queue = new Queue<TreeNode>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var memberWithFamily = new IndividualWithFamily
                {
                    Person = current.Individual,
                    IsSpouse = false,
                    Children = current.Children.Select(c => c.Individual)
                                  .OrderBy(c => SafeParseRank(c.Rank))
                                  .ToList()
                };

                result.Add(memberWithFamily);
                // 添加子节点到队列
                foreach (var child in current.Children)
                {
                    queue.Enqueue(child);
                }
            }
        }

        //遍历家族树，并重写世代数
        private void LevelOrderTraversalWithGeneration(TreeNode root, List<IndividualWithFamily> result)
        {
            if (root == null) return;

            var queue = new Queue<(TreeNode node, int generation)>();
            queue.Enqueue((root, 1)); // 根节点为第1代

            while (queue.Count > 0)
            {
                var (current, currentGeneration) = queue.Dequeue();

                // 修正当前成员的世代
                current.Individual.Generation = currentGeneration.ToString();

                var memberWithFamily = new IndividualWithFamily
                {
                    Person = current.Individual,
                    IsSpouse = false,
                    Children = current.Children.Select(c => c.Individual)
                                      .OrderBy(c => SafeParseRank(c.Rank))
                                      .ToList()
                };

                result.Add(memberWithFamily);

                // 子节点世代为当前世代+1
                foreach (var child in current.Children)
                {
                    queue.Enqueue((child, currentGeneration + 1));
                }
            }
        }
        //辅助构造家族树
        private class TreeNode
        {
            public Individual Individual { get; set; }
            public List<TreeNode> Children { get; } = new List<TreeNode>();
        }

        //加入配偶
        private void AddSpouses(List<IndividualWithFamily> orderedMembers, List<Individual> allMembers)
        {
            var clanMemberIds = new HashSet<long>(orderedMembers.Select(m => m.Person.Id));

            var spousesToAdd = new Dictionary<long, (IndividualWithFamily spouse, int index)>();

            for (int i = 0; i < orderedMembers.Count; i++)
            {
                var member = orderedMembers[i];

                if (member.Person.SpouseId.HasValue)
                {
                    var spouseId = member.Person.SpouseId.Value;
                    var spouse = allMembers.FirstOrDefault(m => m.Id == spouseId);

                    if (spouse != null && !spousesToAdd.ContainsKey(spouseId))
                    {
                        bool isClanMember = clanMemberIds.Contains(spouseId);
                        bool alreadyInList = orderedMembers.Any(m => m.Person.Id == spouseId);

                        if (!alreadyInList || !isClanMember)
                        {
                            var spouseCopy = new Individual
                            {
                                Id = spouse.Id,
                                Surname = spouse.Surname,
                                Name = spouse.Name,
                                Zi = spouse.Zi,
                                Gender = spouse.Gender,
                                Generation = member.Person.Generation,
                                Rank = spouse.Rank,
                                AdBirth = spouse.AdBirth,
                                AdDeath = spouse.AdDeath,
                                Biography = spouse.Biography,
                                FatherId = spouse.FatherId,
                                MotherId = spouse.MotherId,
                                SpouseId = spouse.SpouseId,
                                GeneId = spouse.GeneId
                            };

                            spousesToAdd[spouseId] = (
                                new IndividualWithFamily
                                {
                                    Person = spouseCopy, 
                                    IsSpouse = !isClanMember,
                                    SpouseOf = member.Person,
                                    Children = new List<Individual>()
                                },
                                i + 1
                            );
                        }
                    }
                }
            }

            // 按降序添加配偶
            foreach (var entry in spousesToAdd.Values.OrderByDescending(x => x.index))
            {
                if (entry.index <= orderedMembers.Count)
                {
                    orderedMembers.Insert(entry.index, entry.spouse);
                }
                else
                {
                    orderedMembers.Add(entry.spouse);
                }
            }
        }

        //获得排行
        private int SafeParseRank(string rank)
        {
            if (string.IsNullOrWhiteSpace(rank))
                return 0; 

            return int.TryParse(rank, out int result)
                ? result 
                : 0;                     
        }

        //是否为先祖
        private bool IsAncester(Individual member)
        {
            return !member.FatherId.HasValue &&
                   !member.MotherId.HasValue &&
                   member.Generation == "1" &&
                   member.Gender == "0";
        }

        public List<IndividualWithInfo> GetOrderedGenealogyWithInfo(string geneId)
        {
            List<IndividualWithFamily> individualsWithFamily = GetOrderedGenealogy(geneId);
            var result = new List<IndividualWithInfo>();

            foreach (var individualWithFamily in individualsWithFamily)
            {
                var individual = individualWithFamily.Person;
                var individualWithInfo = new IndividualWithInfo
                {
                    Person = individual,
                    IsSpouse = individualWithFamily.IsSpouse,
                    SpouseOf = individualWithFamily.SpouseOf,
                    Children = individualWithFamily.Children
                };

                // 构建Info字符串
                individualWithInfo.Info = BuildInfoString(individual, individualWithFamily.Children);

                result.Add(individualWithInfo);
            }

            return result;
        }

        private static string BuildInfoString(Individual individual, List<Individual> children)
        {
            var infoParts = new List<string>();

            // 添加字
            if (!string.IsNullOrWhiteSpace(individual.Zi))
            {
                infoParts.Add($"字{individual.Zi}");
            }

            // 添加父亲和排行信息
            if (individual.Father != null && !string.IsNullOrWhiteSpace(individual.Father.Name))
            {
                string rankText = "";
                switch (individual.Rank)
                {
                    case "1": rankText = "长"; break;
                    case "2": rankText = "次"; break;
                    case "3": rankText = "三"; break;
                    case "4": rankText = "四"; break;
                    case "5": rankText = "五"; break;
                    case "6": rankText = "六"; break;
                    case "7": rankText = "七"; break;
                    case "8": rankText = "八"; break;
                    case "9": rankText = "九"; break;
                    case "10": rankText = "十"; break;
                    default: rankText = individual.Rank; break; 
                }
                var rankPart = string.IsNullOrWhiteSpace(rankText) ? "" : rankText;
                infoParts.Add($"{individual.Father.Name}{rankPart}子");
            }

            // 处理生卒信息
            if (!string.IsNullOrWhiteSpace(individual.CeBirth) || !string.IsNullOrWhiteSpace(individual.CeDeath))
            {
                var birthInfo = !string.IsNullOrWhiteSpace(individual.CeBirth) ? $"生于{individual.CeBirth}" : "";
                var deathInfo = !string.IsNullOrWhiteSpace(individual.CeDeath) ? $"殁于{individual.CeDeath}" : "";

                if (!string.IsNullOrEmpty(birthInfo)) infoParts.Add(birthInfo);
                if (!string.IsNullOrEmpty(deathInfo)) infoParts.Add(deathInfo);
            }
            else
            {
                infoParts.Add("生卒不详");
            }

            // 处理配偶信息
            if (individual.Spouse != null)
            {
                var spousePrefix = individual.Gender == "M" ? "婚" : "婚";
                var spouseName = $"{individual.Spouse.Surname}{individual.Spouse.Name}";
                infoParts.Add($"{spousePrefix}{spouseName}");
            }

            // 添加传记
            if (!string.IsNullOrWhiteSpace(individual.Biography))
            {
                infoParts.Add(individual.Biography);
            }

            // 处理子女信息
            if (children.Any())
            {
                var sons = children.Where(c => c.Gender == "M").ToList();
                var daughters = children.Where(c => c.Gender == "F").ToList();

                if (sons.Any())
                {
                    var sonNames = string.Join(" ", sons.Select(s => s.Name));
                    infoParts.Add($"子{sonNames}");
                }

                if (daughters.Any())
                {
                    var daughterNames = string.Join(" ", daughters.Select(d => d.Name));
                    infoParts.Add($"女{daughterNames}");
                }
            }

            return string.Join("，", infoParts);
        }
    }
}
