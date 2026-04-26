export default function StatCard({
  title,
  value,
  subtitle,
  accent = "green",
}) {
  const accents = {
    green: "bg-[#EAF7EC] text-[#4F9A61]",
    yellow: "bg-[#FFF8E8] text-[#C59A2E]",
    red: "bg-[#FFF4F4] text-[#D87474]",
    gray: "bg-[#F4F7F4] text-[#7A8F84]",
  };

  return (
    <div className="rounded-[28px] border border-[#D9E7DB] bg-white p-5 shadow-[0_12px_30px_rgba(111,193,122,0.07)]">
      <div
        className={`mb-4 inline-flex rounded-full px-3 py-1.5 text-sm font-semibold ${accents[accent] || accents.green}`}
      >
        {title}
      </div>
      <div className="text-[34px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
        {value}
      </div>
      {subtitle ? (
        <div className="mt-3 text-[14px] font-medium text-[#7A8F84]">
          {subtitle}
        </div>
      ) : null}
    </div>
  );
}
