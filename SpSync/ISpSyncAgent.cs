namespace Navicon.SP.Components.SqlCache.SpSync
{
    using System;

    using Microsoft.SharePoint;

    // регистрируем все обработчики, создаём БД и таблицы
    // если контент тип то все дочерние списки копируем в одну таблицу под названием контент типа
    // отключаем синхронизацию
    internal interface ISpSyncAgent : IDisposable
    {
        bool RegisterListToSync(SPList spList, bool ovverideView, bool forceCreating, bool hidden = false);
        bool RegisterCtToSync(SPContentType contentType, bool overrideView, bool forceCreating, bool tableNameAsParentList = true, bool hidden = false);
        bool Disable(dynamic ctOrList);
    }
}