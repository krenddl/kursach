export default function BarChart({
  data,
  series,
  heightClass = "h-[220px]",
  emptyMessage = "За выбранный период пока нет данных.",
}) {
  const primarySerie = series?.[0];
  if (!primarySerie) return null;

  const values = data.map((item) => Number(item?.[primarySerie.key] || 0));
  const hasData = values.some((value) => value > 0);
  const maxValue = Math.max(1, ...values);

  if (!hasData) {
    return (
      <div>
        <div
          className={`flex items-center justify-center rounded-[24px] border border-dashed border-[#DFE6E0] bg-[#FAFCFB] px-6 text-center text-[15px] font-medium text-[#8A94A6] ${heightClass}`}
        >
          {emptyMessage}
        </div>

        <div className="mt-4 flex flex-wrap gap-4">
          <div className="inline-flex items-center gap-2 text-sm font-medium text-[#6F8278]">
            <span className={`h-3 w-3 rounded-full ${primarySerie.colorClass}`} />
            {primarySerie.label}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div>
      <div
        className={`grid items-end gap-3 ${heightClass}`}
        style={{ gridTemplateColumns: `repeat(${data.length}, minmax(0, 1fr))` }}
      >
        {data.map((item) => {
          const value = Number(item?.[primarySerie.key] || 0);
          const barHeight = value > 0 ? Math.max((value / maxValue) * 100, 12) : 0;

          return (
            <div
              key={item.key || item.label}
              className="flex h-full min-w-0 flex-col justify-end gap-3"
            >
              <div className="relative mx-auto flex h-full w-full max-w-[58px] items-end rounded-[16px] bg-[#F3F8F3] px-2 pb-2">
                {value > 0 ? (
                  <div
                    className={`w-full rounded-[12px] shadow-[0_14px_30px_rgba(111,193,122,0.16)] ${primarySerie.colorClass}`}
                    style={{ height: `${barHeight}%` }}
                    title={`${item.label}: ${value}`}
                  />
                ) : null}
              </div>

              <div className="mx-auto min-h-[42px] max-w-[72px] text-center">
                <div className="text-[13px] font-extrabold text-[#4F9A61]">
                  {value}
                </div>
                <div className="mt-1 text-[11px] leading-4 font-semibold text-[#7A8F84]">
                  {item.label}
                </div>
              </div>
            </div>
          );
        })}
      </div>

      <div className="mt-4 flex flex-wrap gap-4">
        <div className="inline-flex items-center gap-2 text-sm font-medium text-[#6F8278]">
          <span className={`h-3 w-3 rounded-full ${primarySerie.colorClass}`} />
          {primarySerie.label}
        </div>
      </div>
    </div>
  );
}
