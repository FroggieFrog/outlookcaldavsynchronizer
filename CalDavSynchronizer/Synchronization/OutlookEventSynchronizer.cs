﻿// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer
// Copyright (c) 2015 Alexander Nimmervoll
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.using System;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CalDavSynchronizer.ChangeWatching;
using CalDavSynchronizer.Implementation.Events;
using GenSync;
using GenSync.Logging;
using GenSync.Synchronization;
using AppointmentId = CalDavSynchronizer.Implementation.Events.AppointmentId;

namespace CalDavSynchronizer.Synchronization
{
  public class OutlookEventSynchronizer<TBtypeEntityId, TBtypeEntityVersion> : IOutlookSynchronizer
  {
    private readonly IPartialSynchronizer<AppointmentId, DateTime, TBtypeEntityId, TBtypeEntityVersion> _synchronizer;

    public OutlookEventSynchronizer (IPartialSynchronizer<AppointmentId, DateTime, TBtypeEntityId, TBtypeEntityVersion> synchronizer)
    {
      if (synchronizer == null)
        throw new ArgumentNullException (nameof (synchronizer));

      _synchronizer = synchronizer;
    }

    public Task Synchronize (ISynchronizationLogger logger)
    {
      return _synchronizer.Synchronize (logger);
    }

    public async Task SynchronizePartial(IEnumerable<IOutlookId> outlookIds, ISynchronizationLogger logger)
    {
      var idExtractor = new IdWithHintExtractor();
      foreach (var outlookId in outlookIds)
        outlookId.Accept(idExtractor);

      await _synchronizer.SynchronizePartial(idExtractor.Ids, new IIdWithHints<TBtypeEntityId, TBtypeEntityVersion>[] {}, logger);
    }

    class IdWithHintExtractor : IOutlookIdVisitor
    {
      public List<IIdWithHints<AppointmentId, DateTime>> Ids { get; } = new List<IIdWithHints<AppointmentId, DateTime>>();

      public void Visit(GenericId id)
      {
        throw new NotSupportedException("Objects other than appointments are not supported!");
      }

      public void Visit(ChangeWatching.AppointmentId id)
      {
        Ids.Add(id.Inner);
      }
    }
  }
}