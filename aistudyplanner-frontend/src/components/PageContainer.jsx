const PageContainer = ({ children }) => {
  return (
    <main className="mx-auto max-w-5xl px-4 py-10">
      {children}
    </main>
  );
};

export default PageContainer;