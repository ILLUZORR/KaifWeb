using Robust.Shared.Timing;
using Robust.Shared.Prototypes;
using Robust.Shared.Map.Components;
using Content.Shared.TimeCycle;
using System.Linq;

namespace Content.Server.TimeCycle;

public sealed partial class TimeCycleSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public bool TryFindMembers(string id, out List<EntityUid> members)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        members = new List<EntityUid>();

        var query = EntityQueryEnumerator<TimeCycleMemberComponent>();

        while (query.MoveNext(out var mapid, out var timeComp))
        {
            if (timeComp.TrackerId == id)
                members.Add(mapid);
        }

        if (members is not null)
            return true;
        else
            return false;
    }

    public override void Update(float frameTime)
    {
        var curTime = _gameTiming.CurTime;
        var query = EntityQueryEnumerator<TimeCycleTrackerComponent>();

        while (query.MoveNext(out var _, out var timeComp))
        {
            if (timeComp.Paused
                || curTime < timeComp.DelayTime)
                continue;

            if (timeComp.TrackerId == null)
                continue;

            // TODO: Rewrite to LinkedEntitiesComponent for better performance
            if (!TryFindMembers(timeComp.TrackerId, out var members))
                continue;

            // Should be used for developing time palletes or for debuging
            // O-o-or... You can cosplay pucchi from JoJo 6 with his 'Made In Heaven'
            timeComp.DelayTime = curTime + (timeComp.SpeedUp ? timeComp.SpeedUpMinuteDuration : timeComp.MinuteDuration);

            // Pass minute of map time
            timeComp.CurrentTime += TimeSpan.FromMinutes(1);

            // Change ambient color for all members maps
            foreach (var mapMemberUid in members)
            {
                if (!TryComp<MapLightComponent>(mapMemberUid, out var mapLightComp))
                    continue;

                UpdateAmbientColor(mapMemberUid, timeComp, mapLightComp);
            }
        }

        base.Update(frameTime);
    }

    private void UpdateAmbientColor(EntityUid mapid, TimeCycleTrackerComponent timeComp, MapLightComponent mapLightComp)
    {
        if (!_prototypeManager.TryIndex(timeComp.PalettePrototype, out TimeCyclePalettePrototype? timeEntries)
            || timeEntries is null)
            return;

        var timeInCycle = GetTimeInCycle(timeComp.CurrentTime);
        mapLightComp.AmbientLightColor = GetInterpolatedColor(timeEntries, timeInCycle);
        Dirty(mapid, mapLightComp);
    }

    // We should convert current 'TimeSpan' (with days) time into one day cycle time (in 24 hours)
    private TimeSpan GetTimeInCycle(TimeSpan timeSpan) =>
        TimeSpan.FromMilliseconds(timeSpan.TotalMilliseconds % TimeSpan.FromHours(24).TotalMilliseconds);

    private Color GetInterpolatedColor(TimeCyclePalettePrototype proto, TimeSpan timeInCycle)
    {
        // If there is no one any time entries of palette - return black
        if (proto.TimeEntries is null)
            return Color.Black;

        var currentTime = timeInCycle.TotalHours;
        var startTime = -1;
        var endTime = -1;

        foreach (KeyValuePair<int, Color> kvp in proto.TimeEntries)
        {
            var hour = kvp.Key;
            var color = kvp.Value;

            if (hour <= currentTime)
                startTime = hour;
            else if (hour >= currentTime && endTime == -1)
                endTime = hour;
        }

        if (startTime == -1)
            startTime = 0;
        else if (endTime == -1)
            endTime = 23;

        var entryStart = proto.TimeEntries[startTime];
        var entryEnd = proto.TimeEntries[endTime];
        var entryProgress = GetEntryProgress(TimeSpan.FromHours(startTime), TimeSpan.FromHours(endTime), timeInCycle);

        return Color.InterpolateBetween(entryStart, entryEnd, entryProgress);
    }

    private float GetEntryProgress(TimeSpan startTime, TimeSpan endTime, TimeSpan currentTime) =>
        ((float)(currentTime.TotalMinutes - startTime.TotalMinutes) / (float)(endTime.TotalMinutes - startTime.TotalMinutes));

}
