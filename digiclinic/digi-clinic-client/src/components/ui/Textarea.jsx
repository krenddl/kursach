export default function Textarea({ className = "", ...props }) {
  return (
    <textarea
      className={`w-full rounded-[18px] border border-[#DCE9DE] bg-[#FCFEFC] px-4 py-3 text-[16px] font-medium text-[#2E463A] outline-none transition focus:border-[#6FC17A] focus:ring-4 focus:ring-[#EAF7EC] ${className}`}
      {...props}
    />
  );
}
