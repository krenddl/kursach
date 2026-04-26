export default function Badge({
  children,
  variant = "green",
  className = "",
}) {
  const styles = {
    green: "bg-[#EAF7EC] text-[#4F9A61] border-[#D5ECD9]",
    yellow: "bg-[#FFF8E8] text-[#C59A2E] border-[#F2E2AE]",
    red: "bg-[#FFF4F4] text-[#D87474] border-[#F1D4D4]",
    gray: "bg-[#F4F7F4] text-[#7A8F84] border-[#E2EAE3]",
  };

  return (
    <span
      className={`inline-flex items-center rounded-full border px-3 py-1.5 text-[14px] font-semibold ${styles[variant]} ${className}`}
    >
      {children}
    </span>
  );
}