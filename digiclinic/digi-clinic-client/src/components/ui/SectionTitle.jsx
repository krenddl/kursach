export default function SectionTitle({ title, subtitle }) {
  return (
    <div>
      <h1 className="text-[56px] leading-[0.98] font-extrabold tracking-[-0.06em] text-[#183B2B]">
        {title}
      </h1>
      {subtitle ? (
        <p className="mt-3 text-[19px] font-medium text-[#6F8278]">
          {subtitle}
        </p>
      ) : null}
    </div>
  );
}