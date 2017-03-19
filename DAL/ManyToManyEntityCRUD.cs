namespace Navicon.SP.Components.SqlCache.DAL
{
    using System.Collections.Generic;

    using DapperExtensions;

    using Microsoft.SharePoint;

    using Navicon.SP.Components.SqlCache.Models;

    public class ManyToManyEntityCRUD : BaseCRUD
    {
        public ManyToManyEntityCRUD(SPSite spSite)
            : base(spSite) {}

        public ManyToManyEntityCRUD() {}

        public StatusValuePair<ManyToManyEntities> Create(long docEntityValueId, long archiveElementValueId)
        {
            const string sql = "EXECUTE [dbo].[P_ArchiveEntity_CreateRelated] @LeftId, @RightId";
            StatusValuePair<ManyToManyEntities> manyToManyEntity = this.StoredProcedureSingle<ManyToManyEntities>(sql,
                new
                {
                    LeftId = docEntityValueId,
                    RightId = archiveElementValueId
                });

            return manyToManyEntity;
        }

        public StatusValuePair<ManyToManyEntities> Delete(long lId, long rId)
        {
            PredicateGroup p = new PredicateGroup
            {
                Operator = GroupOperator.And,
                Predicates = new IPredicate[]
                {
                    Predicates.Field<ManyToManyEntities>(entity => entity.LeftId, Operator.Eq, lId),
                    Predicates.Field<ManyToManyEntities>(entity => entity.RightId, Operator.Eq, rId)
                }
            };

            StatusValuePair<ManyToManyEntities> result = this.Delete<ManyToManyEntities>(p);
            return result;
        }

        public StatusValuePair<ManyToManyEntities> Delete(long archiveId)
        {
            PredicateGroup p = new PredicateGroup
            {
                Operator = GroupOperator.Or,
                Predicates = new IPredicate[]
                {
                    Predicates.Field<ManyToManyEntities>(entity => entity.LeftId, Operator.Eq, archiveId),
                    Predicates.Field<ManyToManyEntities>(entity => entity.RightId, Operator.Eq, archiveId)
                }
            };

            StatusValuePair<ManyToManyEntities> result = this.Delete<ManyToManyEntities>(p);
            return result;
        }

        public StatusValuePair<List<ManyToManyEntities>> SearchLeft(long lId)
        {
            IFieldPredicate p = Predicates.Field<ManyToManyEntities>(entity => entity.LeftId, Operator.Eq, lId);
            StatusValuePair<List<ManyToManyEntities>> result = this.GetList<ManyToManyEntities>(p);
            return result;
        }

        public StatusValuePair<List<ManyToManyEntities>> SearchRight(long rId)
        {
            IFieldPredicate p = Predicates.Field<ManyToManyEntities>(entity => entity.RightId, Operator.Eq, rId);
            StatusValuePair<List<ManyToManyEntities>> result = this.GetList<ManyToManyEntities>(p);
            return result;
        }
    }
}