using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;

namespace Yourbiz.Core
{

  //Relazioni helper for Web Project
  public class RelazioniManager
  {

    private static readonly IRelationService relationService = Current.Services.RelationService;

    /// <summary>
    /// Return all related objects to CurrentObjId
    /// </summary>
    /// <param name="CurrentObjId"></param>
    /// <param name="RelationTypeAlias"></param>
    /// <returns></returns>
    public static List<int> GetAny(int CurrentObjId, string RelationTypeAlias)
    {
      var relType = relationService.GetRelationTypeByAlias(RelationTypeAlias);

      var relationsParents = relationService.GetByParentId(CurrentObjId).Where(x => x.RelationType.Id == relType.Id);
      var relationsChilds = relationService.GetByChildId(CurrentObjId).Where(x => x.RelationType.Id == relType.Id);

      var relatedObjects = relationsParents.Select(x => x.ChildId).ToList();
      relatedObjects.AddRange(relationsChilds.Select(x=>x.ParentId));

      return relatedObjects;
    }

    /// <summary>
    /// Return all related objects by childid to CurrentObjId
    /// </summary>
    /// <param name="CurrentObjId"></param>
    /// <param name="RelationTypeAlias"></param>
    /// <returns></returns>
    public static List<int> GetByChildId(int CurrentObjId, string RelationTypeAlias)
    {
      var relType = relationService.GetRelationTypeByAlias(RelationTypeAlias);

      var relationsChilds = relationService.GetByChildId(CurrentObjId).Where(x => x.RelationType.Id == relType.Id);

      var relatedObjects = relationsChilds.Select(x => x.ParentId).ToList();

      return relatedObjects;

    }

    /// <summary>
    /// Return all related objects by parentid to CurrentObjId
    /// </summary>
    /// <param name="CurrentObjId"></param>
    /// <param name="RelationTypeAlias"></param>
    /// <returns></returns>
    public static List<int> GetByParentId(int CurrentObjId, string RelationTypeAlias)
    {
      var relType = relationService.GetRelationTypeByAlias(RelationTypeAlias);

      var relationsParents = relationService.GetByParentId(CurrentObjId).Where(x => x.RelationType.Id == relType.Id);

      var relatedObjects = relationsParents.Select(x => x.ChildId).ToList();

      return relatedObjects;
    }
  }
}
