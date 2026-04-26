export default function Card({
  children,
  className = "",
  padding = "p-6",
}) {
  return (
    <div
      className={`rounded-[28px] border border-[#D9E7DB] bg-white shadow-[0_12px_30px_rgba(111,193,122,0.07)] ${padding} ${className}`}
    >
      {children}
    </div>
  );
}