const ErrorMessage = ({ message }) => {
  if (!message) return null;

  return (
    <div className="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">
      {message}
    </div>
  );
};

export default ErrorMessage;