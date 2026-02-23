import { cn } from '@/lib/utils';

type PageHeaderProps = {
  title: string;
  description?: string;
  className?: string;
};

export function PageHeader({ title, description, className }: PageHeaderProps) {
  return (
    <header className={cn('page-header', className)}>
      <h1 className="text-2xl font-bold tracking-tight text-foreground md:text-3xl">
        {title}
      </h1>
      {description && (
        <p className="page-description mt-1 text-base text-muted-foreground">
          {description}
        </p>
      )}
    </header>
  );
}
