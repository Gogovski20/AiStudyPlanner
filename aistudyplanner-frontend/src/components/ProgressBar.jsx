const ProgressBar = ({ percentage = 0 }) => {
  const safePercentage = Math.min(Math.max(percentage, 0), 100);

  return (
    <div className="h-3 overflow-hidden rounded-full bg-gray-200">
      <div
        className="h-3 rounded-full bg-blue-600"
        style={{ width: `${safePercentage}%` }}
      />
    </div>
  );
};

export default ProgressBar;